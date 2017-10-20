################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
CPP_SRCS += \
../src/Helper/CommonHelper.cpp \
../src/Helper/CurlHelper.cpp \
../src/Helper/ProcessHelper.cpp \
../src/Helper/XMLParserHelper.cpp 

OBJS += \
./src/Helper/CommonHelper.o \
./src/Helper/CurlHelper.o \
./src/Helper/ProcessHelper.o \
./src/Helper/XMLParserHelper.o 

CPP_DEPS += \
./src/Helper/CommonHelper.d \
./src/Helper/CurlHelper.d \
./src/Helper/ProcessHelper.d \
./src/Helper/XMLParserHelper.d 


# Each subdirectory must supply rules for building sources it contributes
src/Helper/%.o: ../src/Helper/%.cpp
	@echo 'Building file: $<'
	@echo 'Invoking: GCC C++ Compiler'
	g++ -O0 -g3 -Wall -c -fmessage-length=0 -std=c++11 -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


