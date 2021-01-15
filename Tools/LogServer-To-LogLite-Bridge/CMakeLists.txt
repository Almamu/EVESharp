cmake_minimum_required(VERSION 3.17)
project(LogServer_To_LogLite_Bridge C)

set(CMAKE_C_STANDARD 99)

add_executable(LogServer_To_LogLite_Bridge main.c logserver.h loglite.h logserver.c loglite.c)