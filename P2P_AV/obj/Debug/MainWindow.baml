   M S B A M L   `   `   `  ����A  =P2P_AV, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:P2P_AVP2P_AV  5   	       P2P_AV.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:P2P_AV     5   	   -   5          � Title$  
MainWindow��5   	   $	��800��6   $
��1280��6)   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      �� 5      $
��Auto �6   .+�5   
   o� -   5         o�Name$ TopPanel��5      $��0��6   .@�5      m� -   5        $��Настройки��5      |�.  6?   5      �� 5   
   -   5         ��Name$ VideoObject��5      $
��Auto��6>   $
��Auto��6L   $��1��6Y   5   O   � 5   
   -   5         �Name$ BottomPanel��5      $��2��6    5      �� 5      �� 5      �� 5      �� 5      �� 5      .+�5      �� -   5         ��Name$ StackPanelMenu1��5   ?   $��0��6    �� Orientation$ 
Horizontal=�6&   .+�5      �� $��IP (only for server):��6   $��3q�69   �� 5       -   5         ��Name$ AudioIP��5    &   $	��100��6   $��3q�67   	� 5"      -   5         	�Name$" AudioType_Server_RadioButton��5"   g   $��Server��65   $��3q�6F   	 	� 	GroupName$	 	AudioType��6Q   |�.  6�   	� 5#      -	   5        $" AudioType_Client_RadioButton��5#   g   $��Client��65   $��3q�6F   $	 	AudioType��6Q   |�.  6�   �� 5%      -
   5        
 ��Name$
 SoundStartBtn��5%   G   $��Start��6,   $��3q�6<   |�.  6^   5&      �� 5(      -   5        $	 IPs��5(      $ 
Horizontal=�6&   $��3��6?   .+�5)      �� $��	Your IPs:��6   $��3q�6-   5*      �� 5,      $��4��6   $ 
Horizontal=�6&   .+�5-      �� -   5        $
 HideBtn��5-   =   $
��Hide��6   |�.  6N   �� 5.      -   5        $
 EnterFullscreenBtn��5.   }   $1��+Enter fullscreen (press ScrollLock to exit)��6   |�.  6}   �� 5/      $��Output volume: ��6   $��10 0 0 0q�63   �� 50      -   5        $	��100��50       � Minimum$ 0Q�6%    � Maximum$ 1Q�61   |�.  6`   �� 51      $��Image quality: ��6   $��10 0 0 0q�63   �� 52      -   5        $	��100��52      $ 0Q�6%   $	 100Q�61   |�.  6c   �� 53      -   5        $��Controls enabled��53   o    d� 	IsChecked$
 TrueY�6�   |�.  6�   54      55      56      57      