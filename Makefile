#*******************************************************************************
#   Ledger Nano S
#   (c) 2016 Ledger
#
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#*******************************************************************************

ifeq ($(BOLOS_SDK),)
$(error Environment variable BOLOS_SDK is not set)
endif
include $(BOLOS_SDK)/Makefile.defines

APP_LOAD_PARAMS=--path "" --curve secp256k1 $(COMMON_LOAD_PARAMS) --appFlags 0x200
APPNAME = Hello

APPVERSION_M=1
APPVERSION_N=2
APPVERSION_P=0
APPVERSION=$(APPVERSION_M).$(APPVERSION_N).$(APPVERSION_P)

ifeq ($(TARGET_NAME),TARGET_BLUE)
ICONNAME=blue_app_hello.gif
else
	ifeq ($(TARGET_NAME),TARGET_NANOX)
ICONNAME=nanox_app_hello.gif
	else
ICONNAME=nanos_app_hello.gif
	endif
endif

################
# Default rule #
################

all: default

############
# Platform #
############

DEFINES   += OS_IO_SEPROXYHAL IO_SEPROXYHAL_BUFFER_SIZE_B=300
DEFINES   += HAVE_BAGL HAVE_SPRINTF
DEFINES   += HAVE_IO_USB HAVE_L4_USBLIB IO_USB_MAX_ENDPOINTS=6 IO_HID_EP_LENGTH=64 HAVE_USB_APDU
DEFINES   += HAVE_USB_CLASS_CCID
DEFINES   += UNUSED\(x\)=\(void\)x
DEFINES   += APPVERSION=\"$(APPVERSION)\"
# DEFINES 	+= DYNAMIC_LOCK

ifneq ($(TARGET_NAME), TARGET_BLUE)
	DEFINES		  += HAVE_UX_FLOW
endif

ifeq ($(TARGET_NAME),TARGET_NANOX)
# DEFINES       += HAVE_BLE BLE_COMMAND_TIMEOUT_MS=2000
# DEFINES       += HAVE_BLE_APDU # basic ledger apdu transport over BLE

DEFINES       += HAVE_GLO096 HAVE_UX_LEGACY
DEFINES       += HAVE_BAGL BAGL_WIDTH=128 BAGL_HEIGHT=64
DEFINES       += HAVE_BAGL_ELLIPSIS # long label truncation feature
DEFINES       += HAVE_BAGL_FONT_OPEN_SANS_REGULAR_11PX
DEFINES       += HAVE_BAGL_FONT_OPEN_SANS_EXTRABOLD_11PX
DEFINES       += HAVE_BAGL_FONT_OPEN_SANS_LIGHT_16PX
# DEFINES 	  += HAVE_ICON_OLD
endif

# Enabling debug PRINTF
DEBUG:=0
ifneq ($(DEBUG),0)
DEFINES += HAVE_STACK_OVERFLOW_CHECK
ifeq ($(TARGET_NAME),TARGET_NANOX)
DEFINES   += HAVE_PRINTF PRINTF=mcu_usb_printf
else
DEFINES   += HAVE_PRINTF PRINTF=screen_printf
endif
else
DEFINES   += PRINTF\(...\)=
endif

##############
# Compiler #
##############
ifneq ($(BOLOS_ENV),)
$(info BOLOS_ENV=$(BOLOS_ENV))
CLANGPATH := $(BOLOS_ENV)/clang-arm-fropi/bin/
GCCPATH := $(BOLOS_ENV)/gcc-arm-none-eabi-5_3-2016q1/bin/
else
$(info BOLOS_ENV is not set: falling back to CLANGPATH and GCCPATH)
endif
ifeq ($(CLANGPATH),)
$(info CLANGPATH is not set: clang will be used from PATH)
endif
ifeq ($(GCCPATH),)
$(info GCCPATH is not set: arm-none-eabi-* will be used from PATH)
endif

CC       := $(CLANGPATH)clang

#CFLAGS   += -O0
CFLAGS   += -O3 -Os -I/usr/include

AS     := $(GCCPATH)arm-none-eabi-gcc

LD       := $(GCCPATH)arm-none-eabi-gcc
LDFLAGS  += -O3 -Os
LDLIBS   += -lm -lgcc -lc

# import rules to compile glyphs(/pone)
include $(BOLOS_SDK)/Makefile.glyphs

### variables processed by the common makefile.rules of the SDK to grab source files and include dirs
APP_SOURCE_PATH  += src
SDK_SOURCE_PATH  += lib_stusb lib_stusb_impl
ifneq ($(TARGET_NAME),TARGET_BLUE)
#SDK_SOURCE_PATH  += lib_blewbxx lib_blewbxx_impl
SDK_SOURCE_PATH  += lib_ux
endif


load: all
	python -m ledgerblue.loadApp $(APP_LOAD_PARAMS)

delete:
	python -m ledgerblue.deleteApp $(COMMON_DELETE_PARAMS)

# import generic rules from the sdk
include $(BOLOS_SDK)/Makefile.rules

#add dependency on custom makefile filename
dep/%.d: %.c Makefile

listvariants:
	@echo VARIANTS APP hello
