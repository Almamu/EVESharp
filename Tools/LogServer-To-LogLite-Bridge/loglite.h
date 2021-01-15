#ifndef LOGSERVER_TO_LOGLITE_BRIDGE_LOGLITE_H
#define LOGSERVER_TO_LOGLITE_BRIDGE_LOGLITE_H

#include "logserver.h"

enum LogLite_Severity
{
    LogLite_Severity_Info,
    LogLite_Severity_Notice,
    LogLite_Severity_Warning,
    LogLite_Severity_Error
};

struct LogLite_MessageEntry
{
    __int64 pid;
    unsigned __int64 timestamp;
    enum LogLite_Severity severity;
    char machineName [31];
    char channelName[32];
    char executableName[260];
    char moduleName [32];
    char message[256];
};

struct LogLite_LinkedListEntry
{
    struct LogLite_LinkedListEntry* prev;
    struct LogLite_MessageEntry* current;
    struct LogLite_LinkedListEntry* next;
};

extern HANDLE gEventLogLiteStopped;

void LogLite_EnqueueMessage(
        __int64 pid, unsigned __int64 timestamp,
        enum LogServer_MessageLevels severity, const char* machineName,
        const char* facilityName, const char* objectName,
        const char* executableName, const char* message
);
DWORD WINAPI LogLite_MainThread (LPVOID lpParam);
void LogLite_StopThread ();

#endif //LOGSERVER_TO_LOGLITE_BRIDGE_LOGLITE_H
