#include <stdio.h>
#include <windows.h>
#include "logserver.h"
#include "loglite.h"

#define FILE_MAPPING_NAME "EVE"

struct LogServer_Handles gLogServer_Handles;
BOOL gLogServer_KeepRunning = TRUE;
HANDLE gEventLogServerStopped;

void LogServer_StopThread ()
{
    gLogServer_KeepRunning = FALSE;
}

DWORD LogServer_AcquireLock ()
{
    DWORD dStatus = WaitForSingleObject (gLogServer_Handles.hndMutexReceiveReady, INFINITE);

    if (!dStatus || dStatus == WAIT_ABANDONED)
    {
        if (!WaitForSingleObject (gLogServer_Handles.hndControlEVE, 0))
        {
            ResetEvent (gLogServer_Handles.hndControlEVE);
            SetEvent (gLogServer_Handles.hndDataEVE);
        }
    }

    return ReleaseMutex (gLogServer_Handles.hndMutexReceiveReady);
}

DWORD WINAPI LogServer_MainThread (LPVOID lpParam)
{
    HANDLE hndFileMap = CreateFileMapping(INVALID_HANDLE_VALUE, 0, PAGE_READWRITE, 0, sizeof (struct LogServer_FileMapHeader), TEXT(FILE_MAPPING_NAME));

    if (hndFileMap == INVALID_HANDLE_VALUE)
    {
        printf ("Cannot create file mapping, error %lu", GetLastError ());
        return 1;
    }

    // map the view of the file
    LPDWORD pBuf = (LPDWORD) MapViewOfFile (hndFileMap, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, sizeof (struct LogServer_FileMapHeader));

    if (pBuf == NULL)
    {
        printf ("Could not map view of file (%lu)", GetLastError ());
        CloseHandle (hndFileMap);

        return 1;
    }

    // set memory area to zero
    memset (pBuf, 0, sizeof (struct LogServer_FileMapHeader));

    struct LogServer_FileMapHeader* map = (struct LogServer_FileMapHeader*) pBuf;

    map->magic = 0x70000;
    map->maxMessageCount = 100;
    map->currentMessageCount = 0;

    gLogServer_Handles.hndControlEVE = CreateEvent(NULL, TRUE, FALSE, "cEVE");
    gLogServer_Handles.hndDataEVE = CreateEvent(NULL, TRUE, FALSE, "dEVE");
    gLogServer_Handles.hndMutexReceiveReady = CreateMutex(NULL, FALSE, "aEVE");
    gLogServer_Handles.hndMutex2 = CreateMutex(NULL, FALSE, "bEVE");

    if (gLogServer_Handles.hndControlEVE == INVALID_HANDLE_VALUE || gLogServer_Handles.hndDataEVE == INVALID_HANDLE_VALUE ||
        gLogServer_Handles.hndMutexReceiveReady == INVALID_HANDLE_VALUE || gLogServer_Handles.hndMutex2 == INVALID_HANDLE_VALUE)
    {
        printf ("Cannot create events and mutexes! %lu", GetLastError ());

        if (gLogServer_Handles.hndControlEVE)
            CloseHandle (gLogServer_Handles.hndControlEVE);
        if (gLogServer_Handles.hndDataEVE)
            CloseHandle (gLogServer_Handles.hndDataEVE);
        if (gLogServer_Handles.hndMutexReceiveReady)
            CloseHandle (gLogServer_Handles.hndMutexReceiveReady);
        if (gLogServer_Handles.hndMutex2)
            CloseHandle (gLogServer_Handles.hndMutex2);

        UnmapViewOfFile (pBuf);
        CloseHandle (hndFileMap);

        return 1;
    }

    struct LogServer_MachineInformation machineInformation;

    memset (&machineInformation, 0, sizeof (struct LogServer_MachineInformation));

    while (gLogServer_KeepRunning == TRUE)
    {
        // wait 500 milliseconds
        Sleep(500);

        // wait for the client to signal some data is ready
        LogServer_AcquireLock();

        // try to show messages from the clone, there should be a few with the info we want
        for (int i = 0; i < map->currentMessageCount; i ++)
        {
            struct LogServer_MessageHeader* msg = &map->messages [i];
            struct LogServer_MapModuleInfo* module = &map->modules[msg->moduleID - 1];

            switch (msg->type)
            {
                case 1:
                    memcpy (&machineInformation, &msg->machine, sizeof (struct LogServer_MachineInformation));
                    break;
                case 2:
                    // queue the mesage to loglite
                    LogLite_EnqueueMessage(
                        msg->msg.pid, msg->timestamp, msg->msg.severity,
                        machineInformation.computerName, module->facilityName, module->facilityObject,
                        machineInformation.parentModuleName, msg->msg.message
                    );
                     break;
                default:
                    printf ("Unknown type %d\n", msg->type);
                    break;
            }
        }

        // all messages read, roll up the indexes
        map->currentMessageCount = 0;
    }

    UnmapViewOfFile(pBuf);

    CloseHandle (gLogServer_Handles.hndControlEVE);
    CloseHandle (gLogServer_Handles.hndDataEVE);
    CloseHandle (gLogServer_Handles.hndMutexReceiveReady);
    CloseHandle (gLogServer_Handles.hndMutex2);

    CloseHandle(hndFileMap);

    // set event so the main thread stops
    SetEvent(gEventLogServerStopped);

    return 0;
}