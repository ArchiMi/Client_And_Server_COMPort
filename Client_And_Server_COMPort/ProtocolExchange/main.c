#define F_CPU 16000000UL

#include <avr/io.h>
#include <util/delay.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <avr/interrupt.h>
#include <stdbool.h>
#include "Src/Crc8.h"

typedef unsigned char byte;

// In table 'Examples of UBRRn Settings' for ATmega328P U2X=1 "7 - 256000 Baud" or 207 - 9600
//#define BAUD_PRESCALE 207 //207 - 9600 U2X=1
#define BAUD_PRESCALE 3 //3 - 256000 U2X=0

#define FRAME_SIZE 32
#define CHR_CARRET_RETURN 13
#define CHR_LINE_FEED 10
#define CHR_COLON 58

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
	UCSR0B = (1<<RXEN0) | (1<<TXEN0); //Разрешаем прием и передачу по USART - T/R ENable = True
	UCSR0C = (0<<UMSEL01) | (0<<UMSEL00) | (1<<USBS0) | (1<<UCSZ00) | (1<<UCSZ01) | (1 << UCSZ00);
	
	UBRR0L = BAUD_PRESCALE;
	UBRR0H = BAUD_PRESCALE >> 8;

	_delay_ms(20);
}

byte USART_Receive(void) {	
	while( !(UCSR0A & (1<<RXC0)) );
	return UDR0;	
}

void USART_Receive_Str(byte *calledstring) {
	char ch;
	
	int i = 0;	
	while(1) {		
		ch = USART_Receive();
		if (ch == CHR_LINE_FEED) {
			calledstring[i] = CHR_LINE_FEED;	
			return;
		} else {
			calledstring[i] = ch;			
		}
		
		/*
		if (i > 0 && ch == CHR_LINE_FEED) {
			if (calledstring[--i] == CHR_CARRET_RETURN) { //Проверить
				calledstring[i] = CHR_LINE_FEED;
				return;
			} else {
				calledstring[i] = ch;
			}
		} else {
			calledstring[i] = ch;
		}
		*/
		
		i++;
	}	
}

void USART_Send(byte data) {
	while( !(UCSR0A & (1<<UDRE0)) );
	UDR0 = data;
}

void USART_Transmit_Str(byte *calledstring) {
	for (int i = 0; i < FRAME_SIZE; i++) {
		if (calledstring[i] != 0)
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

void Clean_Data(byte *input_data) {
	for (uint8_t i = 0; i < FRAME_SIZE; i++) {
		input_data[i] = 0;
	}
}

ISR(WDT_vect) {
	wdt_reset();

	USART_Init();
	
	blink_WD();
}

int main(void) {
	DDRB = 0xFF; //PORTC as Output
	
	byte input[FRAME_SIZE] = { 0 };
	USART_Init();       
	
	while(1) {
		// Get message
		USART_Receive_Str(input);
		
		// Get CRC8 index
		byte index_crc8 = GetCRC8Index(input, FRAME_SIZE, CHR_COLON);
		
		// Information Blink
		blink();
		
		// Add CRC8
		input[index_crc8] = Crc8(input, index_crc8);
		
		// Send answer
		USART_Transmit_Str(input);
		USART_Transmit_Str("\r\n");
		
		// Clean variable
		Clean_Data(input);
	}
	
	return 0;
}