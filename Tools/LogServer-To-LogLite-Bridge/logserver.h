#ifndef LOGSERVER_TO_LOGLITE_BRIDGE_LOGSERVER_H
#define LOGSERVER_TO_LOGLITE_BRIDGE_LOGSERVER_H

#include <windows.h>

enum LogServer_MessageLevels
{
    LogServer_MessageLevel_Info = 1,
    LogServer_MessageLevel_Warning = 2,
    LogServer_MessageLevel_Error = 4,
    LogServer_MessageLevel_Fatal = 8,
    LogServer_MessageLevel_Overlap = 16,
    LogServer_MessageLevel_Performance = 32,
    LogServer_MessageLevel_Counter = 64,
    LogServer_MessageLevel_MethodCall = 128,
};

struct LogServer_MapModuleInfo
{
    int facilityHash;
    int unknown;
    int objectHash;
    char pad0[4];
    char facilityName[32];
    char unknown1;
    char facilityObject[32];
    char pad1[3];
    int muted;
    char pad2[6];
};

struct LogServer_MachineInformation
{
    char computerName[31];
    char parentModuleName[260];
    int processID;
    int instDll;
    char currentModuleName[260];
    long startTime;
    int unk1;
};

struct LogServer_MessageType2
{
    int severity;
    char message[256];
    int pid;
};

struct LogServer_MessageHeader
{
    short moduleID;
    short unk2;
    int threadID;
    unsigned __int64 timestamp;
    int type;
    int messageID;
    union
    {
        struct LogServer_MachineInformation machine;
        struct LogServer_MessageType2 msg;
    };
};

struct LogServer_FileMapHeader
{
    int magic;
    int maxMessageCount;
    short currentMessageCount;
    short unknown;
    short unk1;
    short unk2;
    short lastAvailableMessageID;
    short unk4;
    short moduleCount;
    char pad3[2];
    struct LogServer_MapModuleInfo modules[1000];
    struct LogServer_MessageHeader messages[2500];
};

struct LogServer_Handles
{
    HANDLE hndControlEVE;
    HANDLE hndDataEVE;
    HANDLE hndMutexReceiveReady;
    HANDLE hndMutex2;
};

extern BOOL gLogServer_KeepRunning;
extern HANDLE gEventLogServerStopped;

DWORD WINAPI LogServer_MainThread (LPVOID lpParam);
void LogServer_StopThread ();

#endif //LOGSERVER_TO_LOGLITE_BRIDGE_LOGSERVER_H
