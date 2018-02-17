cmake_minimum_required(VERSION 3.0)
project(Emse_Updater_Linux)

set(CMAKE_CXX_FLAGS ${CMAKE_CXX_FLAGS} )

set(SOURCE_FILES
        main.cpp
        pugixml.cpp
        Helper/CommonHelper.cpp
        Helper/CurlHelper.cpp
        Helper/ProcessHelper.cpp
        App.cpp
        HeaderFiles/Configuration.h
        Helper/LogHelper.cpp
        HeaderFiles/CommonHelper.h
        HeaderFiles/App.h
        HeaderFiles/CurlHelper.h
        HeaderFiles/ProcessHelper.h
        HeaderFiles/LogHelper.h
        )

add_executable(Emse_Updater_Linux ${SOURCE_FILES})

#Boost
find_package(Boost COMPONENTS system filesystem thread REQUIRED)
IF (Boost_FOUND)
    include_directories(${Boost_INCLUDE_DIR})
    target_link_libraries(Emse_Updater_Linux ${Boost_LIBRARIES})
    SET(CMAKE_CXX_FLAGS " -pthread")
    set(Boost_USE_MULTITHREADED ON)
    set(BOOST_ALL_DYN_LINK      ON)

endif()

if (CMAKE_VERSION VERSION_LESS "3.1")
    if (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
      set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=gnu++11")
    endif ()
  else ()
    set_property(TARGET Emse_Updater_Linux PROPERTY CXX_STANDARD 11)
    set_property(TARGET Emse_Updater_Linux PROPERTY CXX_STANDARD_REQUIRED ON)
  endif ()

find_package(CURL REQUIRED)
include_directories(${CURL_INCLUDE_DIR})
target_link_libraries(Emse_Updater_Linux ${CURL_LIBRARIES})
SET(CURL_LIBRARY "-lcurl")

include_directories(${PROJECT_SOURCE_DIR}/HeaderFiles/)

add_custom_command(TARGET Emse_Updater_Linux POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy
        ${CMAKE_SOURCE_DIR}/Configuration.xml
        ${CMAKE_CURRENT_BINARY_DIR}/Configuration.xml)

add_custom_command(TARGET Emse_Updater_Linux POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy
        ${CMAKE_SOURCE_DIR}/version.txt
        ${CMAKE_CURRENT_BINARY_DIR}/version.txt)

include_directories(${PROJECT_SOURCE_DIR}/spdlog/)