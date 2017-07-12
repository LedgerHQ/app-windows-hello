/*******************************************************************************
*   Ledger Nano S - Secure firmware
*   (c) 2016, 2017 Ledger
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
********************************************************************************/

#ifndef UX_NANOS_H
#define UX_NANOS_H

#include "ux_common.h"
#include "glyphs.h"

extern uint8_t icon_hack_flag;

void ui_idle_init(void);
void menu_settings_confirm_login_change_nanos(uint32_t confirm);
void menu_settings_dlock_change_nanos(uint32_t confirm);
void menu_settings_confirm_login_init_nanos(unsigned int ignored);
void menu_settings_dlock_init_nanos(unsigned int ignored);
void ui_confirm_login_init(void);
void ui_confirm_registration_init(void);
void icon_change_callback(unsigned int ignored);

#endif //UX_NANOS_H
