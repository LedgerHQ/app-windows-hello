#ifndef UX_COMMON_H
#define  UX_COMMON_H

#include "os.h"

extern unsigned int ux_step;
extern unsigned int ux_step_count;
extern unsigned char device_key[32];
extern unsigned char auth_key[32];

typedef struct internalStorage_t {
// #define STORAGE_MAGIC 0xDEAD1337
//     uint32_t magic;
    uint32_t dont_confirm_login;
    uint32_t dynamic_lock;
} internalStorage_t;

extern WIDE internalStorage_t N_storage_real;
#define N_storage (*(WIDE internalStorage_t *)PIC(&N_storage_real))
#define SW_OK 0x9000
#define LOGIN_DENIED_BY_USER 0x6984
#define SW_CONDITIONS_NOT_SATISFIED 0x6985

#endif // UX_COMMON_H
