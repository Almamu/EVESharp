#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include "loglite.h"

#pragma comment(lib, "ws2_32.lib")

#define LOGLITE_HOSTNAME "localhost"
#define LOGLITE_PORT "3273"

enum MessageType
{
    CONNECTION_MESSAGE,
    SIMPLE_MESSAGE,
    LARGE_MESSAGE,
    CONTINUATION_MESSAGE,
    CONTINUATION_END_MESSAGE,
};

struct LogLite_ConnectionMessage
{
    unsigned __int32 version;
    __int64 pid;
    char machineName[32];
    char executablePath[260];
};

struct LogLite_MessageMessage
{
    unsigned __int64 timestamp;
    unsigned __int32 severity;
    char module[32];
    char channel[32];
    char message[256];
};

struct LogLite_RawLogMessage
{
    unsigned __int32 type;
    union
    {
        struct LogLite_ConnectionMessage connection;
        struct LogLite_MessageMessage text;
    };
};

struct LogLite_LinkedListEntry* head = NULL;
struct LogLite_LinkedListEntry* tail = NULL;
struct LogLite_LinkedListEntry* nextFree = NULL;

HANDLE gEventLogLiteStopped;
HANDLE gQueueMutex;
BOOL gLogLite_KeepRunning = TRUE;

void LogLite_StopThread ()
{
    gLogLite_KeepRunning = FALSE;
}

void LogLite_EnqueueMessage(
    __int64 pid, unsigned __int64 timestamp,
    enum LogServer_MessageLevels severity, const char* machineName,
    const char* facilityName, const char* objectName,
    const char* executableName, const char* message)
{
    // wait for the mutex to be released
    WaitForSingleObject(gQueueMutex, INFINITE);

    if (nextFree == NULL)
    {
        printf ("No free entries in the loglite queue, skipping message\n");
        return;
    }

    // copy over things to the currentEntry
    strcpy_s (nextFree->current->channelName, 32, objectName);
    strcpy_s (nextFree->current->executableName, 260, executableName);
    strcpy_s (nextFree->current->machineName, 31, machineName);
    strcpy_s (nextFree->current->moduleName, 32, facilityName);
    strcpy_s (nextFree->current->message, 256, message);
    nextFree->current->timestamp = timestamp;
    nextFree->current->pid = pid;

    switch (severity)
    {
        case LogServer_MessageLevel_MethodCall:
            nextFree->current->severity = LogLite_Severity_Notice;
            break;

        case LogServer_MessageLevel_Error:
        case LogServer_MessageLevel_Fatal:
            nextFree->current->severity = LogLite_Severity_Error;
            break;

        case LogServer_MessageLevel_Warning:
            nextFree->current->severity = LogLite_Severity_Warning;
            break;

        case LogServer_MessageLevel_Counter:
        case LogServer_MessageLevel_Info:
        case LogServer_MessageLevel_Overlap:
        case LogServer_MessageLevel_Performance:
        default:
            nextFree->current->severity = LogLite_Severity_Info;
            break;

    }

    nextFree = nextFree->next;

    // release the mutex
    ReleaseMutex(gQueueMutex);
}

void LogLite_InitializeQueue ()
{
    // allocate 2500 entries for the linked list, that should be more than enough
    for (int i = 0; i < 2500; i ++)
    {
        struct LogLite_LinkedListEntry* allocate = (struct LogLite_LinkedListEntry*) malloc (sizeof (struct LogLite_LinkedListEntry));

        // initialize the struct to all zeros to null everything
        memset (allocate, 0, sizeof (struct LogLite_LinkedListEntry));

        // allocate space for the message
        allocate->current = (struct LogLite_MessageEntry*) malloc (sizeof (struct LogLite_MessageEntry));

        // ensure the head is present
        if (head == NULL)
        {
            head = allocate;
        }
        else
        {
            tail->next = allocate;
            allocate->prev = tail;
        }

        // set linked list tail
        tail = allocate;
    }

    nextFree = head;

    // the mutex can be freed as soon as the structures are initialized
    ReleaseMutex (gQueueMutex);
}

void LogLite_SignalStop ()
{
    SetEvent (gEventLogLiteStopped);
}

DWORD WINAPI LogLite_MainThread (LPVOID lpParam)
{
    // initialize winsock first
    WSADATA wsaData;
    int wsaResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

    if (wsaResult != 0)
    {
        printf ("Cannot initialize WinSocket, error: %d\n", wsaResult);

        // signal we're out
        LogLite_SignalStop ();

        return 1;
    }

    LogLite_InitializeQueue ();

    // next open the connection to the loglite app
    struct addrinfo hints;
    struct addrinfo* result = NULL;
    struct addrinfo* addr = NULL;
    memset (&hints, 0, sizeof (struct addrinfo));

    // set hints to get proper data for socket
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    DWORD getAddrInfoRes = getaddrinfo(LOGLITE_HOSTNAME, LOGLITE_PORT, &hints, &result);

    if (getAddrInfoRes != 0)
    {
        printf ("Cannot getaddrinfo for loglite server, error: %lu\n", getAddrInfoRes);

        LogLite_SignalStop ();

        return 2;
    }

    SOCKET sock = INVALID_SOCKET;

    for (addr = result; addr != NULL; addr = addr->ai_next)
    {
        if (addr->ai_family == AF_INET || addr->ai_family == AF_INET6)
        {
            sock = socket (addr->ai_family, SOCK_STREAM, addr->ai_protocol);
            break;
        }
    }

    if (sock == INVALID_SOCKET)
    {
        printf ("Cannot properly startup a socket to loglite, error %lu\n", GetLastError ());

        LogLite_SignalStop ();

        return 3;
    }

    // after all this setup, here's the real deal, connect to it!
    int connectResult = connect (sock, addr->ai_addr, addr->ai_addrlen);

    if (connectResult == SOCKET_ERROR)
    {
        printf ("Cannot connect to loglite server, error: %lu\n", GetLastError ());

        LogLite_SignalStop ();

        return 4;
    }

    printf ("LogLite thread started!\n");

    // build message struct and send it
    struct LogLite_RawLogMessage msg;

    memset (&msg, 0, sizeof (struct LogLite_RawLogMessage));

    BOOL connectionSend = FALSE;

    // connection open! start blasting!
    while (gLogLite_KeepRunning == TRUE)
    {
        // free the mutex
        ReleaseMutex (gQueueMutex);

        // wait 500 ms for incoming data
        Sleep(500);

        // wait for the mutex to be freed
        WaitForSingleObject (gQueueMutex, INFINITE);

        // send the connection message if required
        if (nextFree != head && connectionSend == FALSE)
        {
            msg.connection.version = 2;
            msg.type = CONNECTION_MESSAGE;
            msg.connection.pid = head->current->pid;
            strcpy_s (msg.connection.machineName, 32, head->current->machineName);
            strcpy_s (msg.connection.executablePath, 260, head->current->executableName);

            // send the message
            int sent = 0;
            do
            {
                int res = send(sock, (const char *) &msg, sizeof (struct LogLite_RawLogMessage), 0);

                if (res == -1)
                {
                    LogServer_StopThread ();
                    break;
                }

                if (res)
                    sent += res;

            } while (sent < sizeof (struct LogLite_RawLogMessage));

            connectionSend = TRUE;
        }

        // process the queue
        for (struct LogLite_LinkedListEntry* cur = head; cur != nextFree; cur = cur->next)
        {
            msg.type = SIMPLE_MESSAGE;
            msg.text.severity = cur->current->severity;
            msg.text.timestamp = cur->current->timestamp;
            strcpy_s (msg.text.channel, 32, cur->current->channelName);
            strcpy_s (msg.text.module, 32, cur->current->moduleName);
            strcpy_s (msg.text.message, 256, cur->current->message);

            int sent = 0;
            do
            {
                int res = send (sock, (const char*) &msg, sizeof (struct LogLite_RawLogMessage), 0);

                if (res == -1)
                {
                    LogServer_StopThread ();
                    break;
                }

                if (res)
                    sent += res;
            } while (sent < sizeof (struct LogLite_RawLogMessage));
        }

        // ensure the next free pointer goes back to the head
        // that way there's no duplicates being sent
        nextFree = head;
    }

    // release the mutex as a last resort
    ReleaseMutex (gQueueMutex);

    LogLite_SignalStop ();

    return 0;
}
