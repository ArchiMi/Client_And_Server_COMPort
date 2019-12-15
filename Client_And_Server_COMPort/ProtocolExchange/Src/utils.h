/*
 * dynamic_array.h
 *
 * Created: 30.11.2019 17:10:47
 *  Author: Developer
 */ 

#include "const.h"
#include "dynamic_array_type.h"
#include <avr/io.h>
#include <util/delay.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <avr/interrupt.h>
#include <stdbool.h>

extern const byte Crc8Table[];

uint8_t crc8(uint8_t *pcBlock, DynamicArray* a, uint8_t len);
uint8_t crc8dy(DynamicArray* a, uint8_t len);
byte getCRC8Index(byte *input_data, DynamicArray* a, uint8_t frame_size);

void initArray(DynamicArray* a, size_t initialSize);
void insertArray(DynamicArray* a, byte element_byte);
void freeArray(DynamicArray* a);

void blink_WD();
void blink();