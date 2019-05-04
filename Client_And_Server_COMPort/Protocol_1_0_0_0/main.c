#define F_CPU 16000000UL

#include <avr/io.h>
#include <util/delay.h>

#define BAUD_PRESCALE 7 //207 (Из таблицы Examples of UBRRn Settings for atmel328p "7 - 256000 Baud")
//#define BAUDRATE 9600 
//#define BAUD_PRESCALLER (((F_CPU / (BAUDRATE * 16UL))) - 1)
#define FRAME_SIZE 32
#define END_LINE 13

void USART_Init() {
	UBRR0H = BAUD_PRESCALE >> 8;
	UBRR0L = BAUD_PRESCALE;
	
	UCSR0A |= (1<<U2X0); //Удвоение частоты
	UCSR0B = (1<<RXEN0) | (1<<TXEN0); //Разрешаем прием и передачу по USART - T/R ENable = True
	UCSR0C = (0<<UMSEL01) | (0<<UMSEL00) | (1<<USBS0) | (1<<UCSZ00) | (1<<UCSZ01) | (1 << UCSZ00);
}

unsigned char USART_Receive(void) {	
	while( !(UCSR0A & (1<<RXC0)) );
	return UDR0;
}

void USART_Receive_Str(char *calledstring) {
	char ch;
	int i = 0;
	
	while(1) {		
		ch = USART_Receive();
		
		if (ch == END_LINE) {
			calledstring[i] = 0;	
			return;
		} else {
			calledstring[i] = ch;
			i++;
		}
	}	
}

void USART_Send(unsigned char data) {
	while( !(UCSR0A & (1<<UDRE0)) );
	UDR0 = data;
}

void USART_Transmit_Str(char *calledstring) {
	for (int i = 0; i < FRAME_SIZE; i++) {
		if (calledstring[i] != 0)
			USART_Send(calledstring[i]);
		else 
			break;		
	}
}

void blink(){
	PORTB = 0xFF;
	_delay_ms(100);
	PORTB= 0x00;
	_delay_ms(100);
}

void Clean_Data(char *input_data) {
	for (int i = 0; i < FRAME_SIZE; i++) {
		input_data[i] = 0;
	}
}

int main(void) {
	DDRB = 0xFF; //PORTC as Output
	
	char input[FRAME_SIZE] = { 0 };
	USART_Init();       
	
	while(1) {
		USART_Receive_Str(input);
		
		blink();
		
		char input_str[FRAME_SIZE] = { 0 };
		input_str[0] = '0';
		input_str[1] = '1';
		input_str[2] = '2';
		input_str[3] = '3';
		input_str[4] = '0';
		input_str[5] = '1';
		input_str[6] = '2';
		input_str[7] = '3';
			
		USART_Transmit_Str(input_str);
		USART_Transmit_Str("\r");
		
		Clean_Data(input_str);
	}
	
	return 0;
}