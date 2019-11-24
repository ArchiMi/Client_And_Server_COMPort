#define F_CPU 16000000UL
#define NULL ((void*)0)

#include <avr/io.h>
#include <util/delay.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <avr/interrupt.h>
#include <stdbool.h>
#include "Src/crc8.h"
#include "Src/const.h"

typedef struct {
	byte* array;
	int used;
	int size;
} Array;

void initArray(Array* a, size_t initialSize) {
	a->array = (byte*)malloc(initialSize * sizeof(byte));
	a->used = 0;
	a->size = initialSize;
}

void insertArray(Array* a, byte element_byte) {
	if (a->used == a->size) {
		a->size += 1;
		byte* newarr = (byte*)realloc(a->array, a->size * sizeof(byte));
		if (newarr == NULL) return; // Block don't selected - return 
		a->array = newarr;
	}

	a->array[a->used++] = element_byte;
}

void freeArray(Array* a) {
	free(a->array);
	a->array = NULL;
	a->used = a->size = 0;
}



void USART_Init() {
	sei();
	
	//BEGIN Test	
	wdt_disable();
	
	bool WD_RST = MCUSR & 0x08;
	bool BO_RST = MCUSR & 0x04;
	bool EXT_RST = MCUSR & 0x02;
	bool PON_RST = MCUSR & 0x01;
	MCUSR = MCUSR & 0xF0;
	//END Test
		
	wdt_enable(WDTO_8S);
	
	WDTCSR = 1<<WDIE;
	
	//UCSR0A |= (1<<U2X0); //Удвоение частоты
	UCSR0B = (1<<RXEN0) | (1<<TXEN0); //Trust transmit and receive from USART - T/R ENable = True
	UCSR0C = (0<<UMSEL01) | (0<<UMSEL00) | (1<<USBS0) | (1<<UCSZ00) | (1<<UCSZ01) | (1 << UCSZ00);
	
	UBRR0L = BAUD_PRESCALE;
	UBRR0H = BAUD_PRESCALE >> 8;

	// Не менее 20 милисекунд
	_delay_ms(20);
}

byte USART_Receive(void) {	
	while( !(UCSR0A & (1<<RXC0)) );
	return UDR0;	
}

void USART_Receive_Str(byte *calledstring, Array* a) {
	char ch;
	
	int i = 0;	
	while(1) {				
		// Check error from receive 
		if (i == FRAME_SIZE) {
			return;
		}		
	
		// Get input char	
		ch = USART_Receive();
		
		// Check first char equal COLON
		if (ch != CHR_COLON && i == 0) {
			continue;
		} else {
			if (ch == CHR_LINE_FEED && i > 0) {
				int previous_index = i - 1;
 				if (calledstring[previous_index] == CHR_CARRET_RETURN) {
					calledstring[i] = CHR_LINE_FEED;
					insertArray(a, CHR_LINE_FEED);
					return;
				} else {
					calledstring[i] = ch;
					insertArray(a, ch);
				}
			} else {
				calledstring[i] = ch;
				insertArray(a, ch);
			}	
		}
		
		i++;
	}	
}

void USART_Send(byte data) {
	while( !(UCSR0A & ( 1 << UDRE0 )) );
	UDR0 = data;
}

// ERROR ERROR ... called string may be one char, but FRAME_SIZE 32 chars
void USART_Transmit_Str(byte *calledstring) {
	// Chars array
	for (int i = 0; i <= FRAME_SIZE; i++) {
		if (calledstring[i] != 0)
			// Send char
			USART_Send(calledstring[i]);
		else 
			break;		
	}
}

void blink_WD() {
	PORTB |= ( 1 << PINB4 ); //0xFF; //On
	_delay_ms(1);
	PORTB &= ~( 1 << PINB4 ); //0x00; //OFF
	_delay_ms(1);
}

void blink() {
	PORTB |= ( 1 << PINB5 ); //0xFF; //On
	_delay_ms(5);
	PORTB &= ~( 1 << PINB5 ); //0x00; //OFF
	_delay_ms(5);
}

void blinkDebig() {
	PORTB |= ( 1 << PINB6 ); //0xFF; //On
	_delay_ms(5);
	PORTB &= ~( 1 << PINB6 ); //0x00; //OFF
	_delay_ms(5);
}

void Clean_Data(byte *input_data) {
	for (uint8_t i = 0; i < FRAME_SIZE; i++) {
		input_data[i] = 0;
	}
}

// Watch dog
ISR(WDT_vect) {
	wdt_reset();

	USART_Init();
	
	blink_WD();
}

void Testfunc() {
	Array a;
	int i;
				
	initArray(&a, 5);  // Initially 5 elements
	for (i = 0; i < 100; i++) {
		insertArray(&a, i);  // Automatically resizes as necessary
	}
	
	int count = a.size;
	for (i = 0; i < count; i++) {
		a.array[i] = i;
	}
				
	//a.array[9]  // Get and set byte to array element
	//a.used  // Number of elements
	freeArray(&a);
}

int main(void) {
	DDRB = 0xFF; //PORTB as Output
	
	byte input[FRAME_SIZE] = { 0 };
	
	USART_Init();       
	
	while(1) {
		// Init Array
		Array inputArray;
		initArray(&inputArray, 1000);
		
		// Get request
		USART_Receive_Str(input, &inputArray);
		
		// Get CRC8 byte index
		byte index_crc8 = GetCRC8Index(input, FRAME_SIZE, CHR_COLON);
			
		// Add CRC8 byte
		byte crc8 = Crc8(input, index_crc8);
		//input[index_crc8] = crc8;
		
		if (input[index_crc8] == crc8) {
			
			// Info Blink
			//blink();
						
			// TEST TEMP FUNC
			//Testfunc();
			
			/*
			// Send response
			byte output[FRAME_SIZE] = { 0 };			
			output[0] = CHR_COLON;
			output[1] = 1;
			output[2] = 2;
			output[3] = 3;
			output[4] = 4;
			output[5] = 5;
			output[6] = 6;
			output[7] = CHR_COLON;
			output[8] = CHR_CARRET_RETURN;
			output[9] = CHR_LINE_FEED;
			USART_Transmit_Str(output);
			USART_Transmit_Str(CHR_CARRET_RETURN);
			*/
			
			// Send response ( ECHO )
			USART_Transmit_Str(input);
			//USART_Transmit_Str(CHR_COLON);
			USART_Transmit_Str(CHR_CARRET_RETURN);
			//USART_Transmit_Str(CHR_LINE_FEED);
			
			// Info Blink
			//blinkDebig();
			
		} else /* ERROR */ {
			// Info Blink
			//blink();
	
			byte answer[FRAME_SIZE] = { 0 };
			answer[0] = CHR_COLON;
			answer[1] = crc8;
			answer[2] = input[index_crc8];
			answer[3] = index_crc8;
			answer[4] = "2";
			answer[5] = "3";
			answer[6] = "4";
			answer[7] = CHR_COLON;
			answer[8] = CHR_CARRET_RETURN;
			answer[9] = CHR_LINE_FEED;
			
			// Send response
			USART_Transmit_Str(answer);
			
			// End answer message chars
			USART_Transmit_Str(CHR_CARRET_RETURN);
			USART_Transmit_Str(CHR_LINE_FEED);
		}
		
		// Clear array
		Clean_Data(input);
		
		// Free Array
		freeArray(&inputArray);	
		
		// Info Blink
		blink();
		
		// Info Blink
		//blinkDebig();
	}
	
	return 0;
}