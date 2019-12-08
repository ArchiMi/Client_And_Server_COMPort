/*
 * dynamic_array.h
 *
 * Created: 30.11.2019 17:10:47
 *  Author: Developer
 */ 

#include "const.h"
#include "dynamic_array_type.h"
#include <avr/pgmspace.h>

void initArray(Array* a, size_t initialSize);
void insertArray(Array* a, byte element_byte);
void freeArray(Array* a);