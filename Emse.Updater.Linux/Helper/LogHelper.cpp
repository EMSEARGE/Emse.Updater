//
// Created by root on 16.02.2018.
//

#include <sstream>
#include "LogHelper.h"
#include <boost/date_time/gregorian/gregorian.hpp>
#include <boost/filesystem.hpp>

#define GetCurrentDir getcwd

namespace  spd = spdlog;
using namespace spd;

std::shared_ptr<logger> sysLogger;

std::string utc_date()
{
    namespace bg = boost::gregorian;

    static char const* const fmt = "%d%m%y";
    std::ostringstream ss;
    // assumes std::cout's locale has been set appropriately for the entire app
    ss.imbue(std::locale(std::cout.getloc(), new bg::date_facet(fmt)));
    ss << bg::day_clock::universal_day();

    return ss.str();
}


std::string CurrentDir()
{
    char cCurrentPath[FILENAME_MAX];

    if (!GetCurrentDir(cCurrentPath, sizeof(cCurrentPath)))
    {
        return "";
    }

    cCurrentPath[sizeof(cCurrentPath) - 1] = '\0'; /* not really required */

    return (std::string) cCurrentPath;
}

void LogHelper::WriteLog(int logType, std::string log)
{
    try
    {
        sysLogger = spd::basic_logger_mt(utc_date(), CurrentDir() + "/logger/LOG-" + utc_date() + ".log");
        spdlog::register_logger(sysLogger);
    }
    catch(...)
    {
        //ignored.
    }

    sysLogger->flush_on(spdlog::level::info);

    try
    {
        switch (logType)
        {
            case INFOLOG:
                //info_logger->info(log);
                sysLogger->info(log);
                break;

            case ERRORLOG:
                //error_logger->error(log);
                sysLogger->error(log);
                break;

            case WARNINGLOG:
                //warning_logger->warn(log);
                sysLogger->warn(log);
                break;

            case CRITICALLOG:
                //critical_logger->critical(log);
                sysLogger->critical(log);
                break;

            default:
                sysLogger->debug(log);
                break;
        }

        spdlog::drop_all();
    }
    catch (...)
    {
        //ignored.
    }
}

void LogHelper::CheckLoggerDir()
{
    try
    {
        if(!boost::filesystem::exists(CurrentDir() + "/logger"))
        {
            if (boost::filesystem::create_directory(CurrentDir() + "/logger"))
            {
                std::cout<<"Dir /logger has been created."<<std::endl;
            }
        }
    }
    catch (...)
    {
        std::cout<<"Boost Filesystem error."<<std::endl;
        throw 0;
    }
}