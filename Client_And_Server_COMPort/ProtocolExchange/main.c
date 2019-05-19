#define F_CPU 16000000UL

#include <avr/io.h>
#include <util/delay.h>
#include <avr/pgmspace.h>

typedef unsigned char byte;

// (�� ������� Examples of UBRRn Settings for ATmega328P "7 - 256000 Baud" ��� 207 - 9600)
//#define BAUD_PRESCALE 207
#define BAUD_PRESCALE 7

#define FRAME_SIZE 32
#define END_LINE 13

const byte Crc8Table[] = {
	0x81, 0x31, 0x62, 0x53, 0xC4, 0xF5, 0xA6, 0x97,
	0xB9, 0x88, 0xDB, 0xEA, 0x7D, 0x4C, 0x1F, 0x2E,
	0x43, 0x72, 0x21, 0x10, 0x87, 0xB6, 0xE5, 0xD4,
	0xFA, 0xCB, 0x98, 0xA9, 0x3E, 0x0F, 0x5C, 0x6D,
	0x86, 0xB7, 0xE4, 0xD5, 0x42, 0x73, 0x20, 0x11,
	0x3F, 0x0E, 0x5D, 0x6C, 0xFB, 0xCA, 0x99, 0xA8,
	0xC5, 0xF4, 0xA7, 0x96, 0x01, 0x30, 0x63, 0x52,
	0x7C, 0x4D, 0x1E, 0x2F, 0xB8, 0x89, 0xDA, 0xEB,
	0x3D, 0x0C, 0x5F, 0x6E, 0xF9, 0xC8, 0x9B, 0xAA,
	0x84, 0xB5, 0xE6, 0xD7, 0x40, 0x71, 0x22, 0x13,
	0x7E, 0x4F, 0x1C, 0x2D, 0xBA, 0x8B, 0xD8, 0xE9,
	0xC7, 0xF6, 0xA5, 0x94, 0x03, 0x32, 0x61, 0x50,
	0xBB, 0x8A, 0xD9, 0xE8, 0x7F, 0x4E, 0x1D, 0x2C,
	0x02, 0x33, 0x60, 0x51, 0xC6, 0xF7, 0xA4, 0x95,
	0xF8, 0xC9, 0x9A, 0xAB, 0x3C, 0x03, 0x5E, 0x6F,
	0x41, 0x70, 0x23, 0x12, 0x85, 0xB4, 0xE7, 0xD6,
	0x7A, 0x4B, 0x18, 0x29, 0xBE, 0x8F, 0xDC, 0xED,
	0xC3, 0xF2, 0xA1, 0x90, 0x07, 0x36, 0x65, 0x54,
	0x39, 0x08, 0x5B, 0x6A, 0xFD, 0xCC, 0x9F, 0xAE,
	0x80, 0xB1, 0xE2, 0xD3, 0x44, 0x75, 0x26, 0x17,
	0xFC, 0xCD, 0x9E, 0xAF, 0x38, 0x09, 0x5A, 0x6B,
	0x45, 0x74, 0x27, 0x16, 0x81, 0xB0, 0xE3, 0xD2,
	0xBF, 0x8E, 0xDD, 0xEC, 0x7B, 0x4A, 0x19, 0x28,
	0x06, 0x37, 0x64, 0x55, 0xC2, 0xF3, 0xA0, 0x91,
	0x47, 0x76, 0x25, 0x14, 0x83, 0xB2, 0xE1, 0xD0,
	0xFE, 0xCF, 0x9C, 0xAD, 0x3A, 0x0B, 0x58, 0x69,
	0x04, 0x35, 0x66, 0x57, 0xC0, 0xF1, 0xA2, 0x93,
	0xBD, 0x8C, 0xDF, 0xEE, 0x79, 0x48, 0x1B, 0x2A,
	0xC1, 0xF0, 0xA3, 0x92, 0x05, 0x34, 0x67, 0x56,
	0x78, 0x49, 0x1A, 0x2B, 0xBC, 0x8D, 0xDE, 0xEF,
	0x82, 0xB3, 0xE0, 0xD1, 0x46, 0x77, 0x24, 0x15,
	0x3B, 0x0A, 0x59, 0x68, 0xFF, 0xCE, 0x9D, 0xAC
};

uint8_t Crc8(uint8_t *pcBlock, uint8_t len) {

	uint8_t crc = 0xFF;
	
	for (int i = 0; i < len; i++) {		
		//crc = pgm_read_byte(&Crc8Table[crc ^ *pcBlock++]);
		crc = Crc8Table[crc ^ *pcBlock++];
	}

	return crc;
}

void USART_Init() {
	UBRR0H = BAUD_PRESCALE >> 8;
	UBRR0L = BAUD_PRESCALE;
	
	UCSR0A |= (1<<U2X0); //�������� �������
	UCSR0B = (1<<RXEN0) | (1<<TXEN0); //��������� ����� � �������� �� USART - T/R ENable = True
	UCSR0C = (0<<UMSEL01) | (0<<UMSEL00) | (1<<USBS0) | (1<<UCSZ00) | (1<<UCSZ01) | (1 << UCSZ00);
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
		
		if (ch == END_LINE) {
			calledstring[i] = END_LINE;	
			return;
		} else {
			calledstring[i] = ch;			
		}
		
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

void blink() {
	PORTB = 0xFF;
	_delay_ms(100);
	PORTB= 0x00;
	_delay_ms(100);
}

void Clean_Data(byte *input_data) {
	for (uint8_t i = 0; i < FRAME_SIZE; i++) {
		input_data[i] = 0;
	}
}

byte GetCRC8Index(byte *input_data) {
	uint8_t x = 0;
	
	for (uint8_t i = 0; i < FRAME_SIZE; i++) {
		if (input_data[i] == END_LINE) {
			return i - 1;	
		}
		
		x++;
	}
	
	return x;
}

int main(void) {
	DDRB = 0xFF; //PORTC as Output
	
	byte input[FRAME_SIZE] = { 0 };
	USART_Init();       
	
	while(1) {
		USART_Receive_Str(input);
		byte index_crc8 = GetCRC8Index(input);
		
		//input[1] = index_crc8;
		
		blink();
		
		/*
		unsigned char input_str[FRAME_SIZE] = { 0 };
		input_str[0] = '0';
		input_str[1] = '1';
		input_str[2] = '2';
		input_str[3] = '3';
		input_str[4] = '4';
		input_str[5] = '5';
		input_str[6] = 254;
		input_str[7] = '7';
		
		unsigned char crc_value = Crc8(input_str, 6);
		input_str[8] = crc_value;
		*/
		
		input[index_crc8] = Crc8(input, index_crc8);
		
		USART_Transmit_Str(input);
		USART_Transmit_Str("\r");

		Clean_Data(input);
	}
	
	return 0;
}