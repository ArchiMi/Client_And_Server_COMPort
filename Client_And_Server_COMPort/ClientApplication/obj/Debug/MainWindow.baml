   M S B A M L   `   `   `  ����D  @ClientApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:ClientAppNameSpaceClientAppNameSpace  5   	   #    ClientAppNameSpace.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:ClientAppNameSpace     5   	     � Title$  Client Application��5   	   $��644.906��5	   	   $��1203.889��5
   	    � WindowState$ 	Maximized=�5   	    � WindowStartupLocation$ CenterScreen=�5   	    � 
ResizeMode$ NoResize=�5   	   .��5      �  � ColumnDefinitions 5   
   �� 5      �� 5      $��0* �6   .+�5   
   �� -   5         ��Name$ 	btn_start��5      $��
Start task��6$   $��
59,157,0,0q�69   $	��Top=�6M   $��22��6z   $
��Left=�6�   $	��130��6�   �� 5   
   -   5         ��Name$ msg_list��5      $��10,465,10,10q�6%    � 	Resources 5       �� 
TargetType	 �� Property
 �� Value �� Triggers �� Binding �� Value��%�  '��       �� 5      " ��6   .�5      �� 	 � ��$

 Left=�6C   �� 5      	 � ��$
 Center=�6A   �� 5      	 � ��$
 0,0,0,-2q�6/    5      �� 5       6&   �� *	IsError+	 False6B   .a�5      �� 	 � ��
 �	#FFCDFB966;   5      �� 5        6&   �� *	IsError+ True6B   .a�5!      �� 	 � ��
 �	#FFF79A9A6;   5"      5$      5%       �� View 5'      �� 5(      .P�5)      �� $��ID��6%    �� Width$ 55��61    �� DisplayMemberBinding 6<   �� *Index+�� 5*      $��Дата/Время��6%   $	 145��69    6E   �� *DateTimeOperation+ �� StringFormat$  {0:dd.MM.yyyy H:mm:ss fff}���� 5+      $��Команда��6%   $ 80��66    6A   �� *CMD+�� 5,      $��Статус��6%   $ 50��65    6@   �� *Status+�� 5-      $��Ошибка��6%   $ 55��65    6@   �� *	IsError+�� 5.      $��Описание��6%   $	 700��67    6C   �� *	Message+�� 5/      $��Balance��6%   $ 50��66    �� CellTemplate 50      �� 51      .R�52   "   ��  �� TextAlignment$ Right=�6E   ��6,   �� *	Balance+6E   53      55      56      58      �� 59   
   -   5        $ 	btn_clear��59      $��	Clean All��6$   $��
59,184,0,0q�68   $	��Top=�6L   $��22��6   $
��Left=�6�   $	��130��6�   �� 5:   
   -   5         ��Name$ cb_ComPorts��5:      $��	89,14,0,0q�6(   $	��Top=�6;   $
��Left=�6S   $	��163��6n   �� 5<   
   $��ComPort:��6   $
��Left=�6#   $��	10,10,0,0q�6>   $	��Top=�6Q   $��74��6i   �� 5=   
   $��Baud:��6   $
��Left=�6    $��	10,41,0,0q�6;   $	��Top=�6N   $��74��6f   �� 5>   
   $��	Stop Bit:��6   $
��Left=�6$   $��	10,72,0,0q�6?   $	��Top=�6R   $��74��6j   �� 5?   
   -   5        $ cb_baud��5?      $��	89,45,0,0q�6$   {�.   �� SelectedIndex$ 7��6I   $	��Top=�6[   $
��Left=�6s   $	��163��6�   .@�5@      �� $
��4800��6   +) 4800, 9600, 19200, 38400, 57600, 115200 �� 5A      $
��9600��6   �� 5B      $��19200��6   �� 5C      $��38400��6   �� 5D      $��57600��6   �� 5E      $��115200��6   �� 5F      $��256000��6   5G      �� 5I   
   -   5        $ cb_stop_bit��5I      {�.  $��	89,76,0,0q�6:   $ 1��6M   $	��Top=�6_   $
��Left=�6w   $	��163��6�   .@�5J      �� $��1��6   �� 5K      $��2��6   5L      �� 5O   
   $��	0,10,10,0q�6   $��Right=�6#   $��Uniform=�6@   $��Image/no_image.png��6R   $	��450��6n   $	��Top=�6{   5P      5Q      