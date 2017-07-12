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

#ifndef USBD_CCID_IMPL_H
#define USBD_CCID_IMPL_H

#define TPDU_EXCHANGE                  0x01
#define SHORT_APDU_EXCHANGE            0x02
#define EXTENDED_APDU_EXCHANGE         0x04
#define CHARACTER_EXCHANGE             0x00

#define EXCHANGE_LEVEL_FEATURE         SHORT_APDU_EXCHANGE
  

#define CCID_BULK_IN_EP 0x82
#define CCID_BULK_EPIN_SIZE 64
#define CCID_BULK_OUT_EP 0x02
#define CCID_BULK_EPOUT_SIZE 64
#define CCID_INTR_IN_EP 0x81
#define CCID_INTR_EPIN_SIZE 16

#define CCID_EP0_BUFF_SIZ            64


#endif // USBD_CCID_IMPL_H