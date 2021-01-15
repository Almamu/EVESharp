#include <windows.h>
#include "logserver.h"
#include "loglite.h"

BOOL WINAPI Bridge_ConsoleCtrlHandler (DWORD dwCtrlType)
{
    if (dwCtrlType == CTRL_C_EVENT || dwCtrlType == CTRL_CLOSE_EVENT)
    {
        LogServer_StopThread ();
        LogLite_StopThread ();
        return TRUE;
    }

    return FALSE;
}

int main()
{
    // set the control handler
    SetConsoleCtrlHandler(Bridge_ConsoleCtrlHandler, TRUE);

    // create required events
    gEventLogServerStopped = CreateEvent(NULL, TRUE, FALSE, "LogServerStopped");

    HANDLE hndLogServerThread = CreateThread(NULL, 0, LogServer_MainThread, NULL, NULL, NULL);
    HANDLE hndLogLiteThread = CreateThread(NULL, 0, LogLite_MainThread, NULL, NULL, NULL);

    // wait for the log server to stop
    WaitForSingleObject (gEventLogServerStopped, INFINITE);
    // wait for loglite to send all the data pending and stop
    WaitForSingleObject (gEventLogLiteStopped, INFINITE);

    return 0;
}
