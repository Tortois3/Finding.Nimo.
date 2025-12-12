using FINDINGNIMO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using static FINDINGNIMO.Animation;
using static FINDINGNIMO.Intro;
using static FINDINGNIMO.Trial1;
using static System.Console;
using static System.Net.Mime.MediaTypeNames;


namespace FINDINGNIMO
{
    public class playerDetails
    {
        public string? userName;
        public static int age;
    }

    public interface IAnimation
    {
        void PlayAnimation();
    }

    public interface IChecks
    {
        void SaveCheckpoint(string choice);
    }

    class Typing
    {
        private const int DefaultDelay = 40;

        public static void Blink(string text, int blinkcnt = 5, int on = 500, int off = 200)
        {
            CursorVisible = false;
            for (int i = 0; i < blinkcnt; i++)
            {
                int left = Math.Max((Console.WindowWidth - text.Length) / 2, 0);
                Console.SetCursorPosition(left, Console.CursorTop);
                Console.WriteLine(text);
                Thread.Sleep(on);
                Console.Clear();
                Thread.Sleep(off);
            }

            int finalLeft = Math.Max((Console.WindowWidth - text.Length) / 2, 0);
            Console.SetCursorPosition(finalLeft, Console.CursorTop);
            Console.WriteLine(text);
            CursorVisible = true;
        }

        public static void AnimateType(string text, ConsoleColor color = ConsoleColor.White, string alignment = "left")
        {
            Console.ForegroundColor = color;
            string[] lines = text.Split('\n');

            foreach (string line in lines)
            {
                if (alignment.ToLower() == "center")
                {
                    int left = Math.Max((Console.WindowWidth - line.Length) / 2, 0);
                    Console.SetCursorPosition(left, Console.CursorTop);
                }
                else if (alignment.ToLower() == "right")
                {
                    int left = Math.Max(Console.WindowWidth - line.Length, 0);
                    Console.SetCursorPosition(left, Console.CursorTop);
                }
                
                for (int i = 0; i < line.Length; i++)
                {
                    Console.Write(line[i]);
                    Thread.Sleep(DefaultDelay);

                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            Console.Write(line.Substring(i + 1));
                            break;
                        }
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        public static void AnimateFrames(string[] frames, int repeat = 1, int delay = 250, int yOffset = 0)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            int eraseWidth = 80;
            int eraseHeight = 6;
            int maxSafeY = Math.Max(0, Console.BufferHeight - eraseHeight);
            int safeY = Math.Clamp(yOffset, 0, maxSafeY);

            for (int i = 0; i < repeat; i++)
            {
                foreach (var frame in frames)
                {
                    string[] lines = frame.Split('\n');

                    for (int j = 0; j < eraseHeight; j++)
                    {
                        int top = safeY + j;
                        if (top >= 0 && top < Console.BufferHeight)
                        {
                            Console.SetCursorPosition(0, top);
                            int writeWidth = Math.Min(eraseWidth, Console.WindowWidth);
                            Console.Write(new string(' ', writeWidth));
                        }
                    }

                    for (int k = 0; k < lines.Length; k++)
                    {
                        int top = safeY + k;
                        if (top >= 0 && top < Console.BufferHeight)
                        {
                            Console.SetCursorPosition(0, top);
                            string toWrite = lines[k];
                            if (toWrite.Length > Console.WindowWidth)
                                toWrite = toWrite.Substring(0, Console.WindowWidth);
                            Console.Write(toWrite);
                        }
                    }

                    int cursorTop = Math.Min(safeY, Console.BufferHeight - 1);
                    Console.SetCursorPosition(0, Math.Max(0, cursorTop));

                    Thread.Sleep(delay);
                }
            }
            Console.CursorVisible = true;
        }

        public static int GetSafeYOffset(int desiredTop, int frameHeight, int eraseHeight = 6)
        {
            int required = Math.Max(frameHeight, eraseHeight);
            int maxTop = Math.Max(0, Console.BufferHeight - required);
            return Math.Clamp(desiredTop, 0, maxTop);
        }

        public static void AnimateJump(string[] frames, int startY, int jumpHeight, int delay = 100)
        {
            int frameHeight = 0;
            foreach (var f in frames) frameHeight = Math.Max(frameHeight, f.Split('\n').Length);
            startY = GetSafeYOffset(startY, frameHeight);
            for (int offset = 0; offset < jumpHeight; offset++)
                AnimateFrames(frames, repeat: 1, delay: delay, yOffset: Math.Max(0, startY - offset));
        }
    }

    public class Animation : IAnimation
    {
        public static string title = @" 
      ███████████ █████ ██████   █████ ██████████   █████ ██████   █████   █████████             ██ ██████   █████ █████ ██████   ██████    ███████    ██
      ▒▒███▒▒▒▒▒▒█▒▒███ ▒▒██████ ▒▒███ ▒▒███▒▒▒▒███ ▒▒███ ▒▒██████ ▒▒███   ███▒▒▒▒▒███        ███▒▒██████ ▒▒███ ▒▒███ ▒▒██████ ██████   ███▒▒▒▒▒███  ███
       ▒███   █ ▒  ▒███  ▒███▒███ ▒███  ▒███   ▒▒███ ▒███  ▒███▒███ ▒███  ███     ▒▒▒       ▒▒▒  ▒███▒███ ▒███  ▒███  ▒███▒█████▒███  ███     ▒▒███▒▒▒ 
       ▒███████    ▒███  ▒███▒▒███▒███  ▒███    ▒███ ▒███  ▒███▒▒███▒███ ▒███                   ▒███▒▒███▒███  ▒███  ▒███▒▒███ ▒███ ▒███      ▒███    
       ▒███▒▒▒█    ▒███  ▒███ ▒▒██████  ▒███    ▒███ ▒███  ▒███ ▒▒██████ ▒███    █████         ▒███ ▒▒██████  ▒███  ▒███ ▒▒▒  ▒███ ▒███      ▒███    
       ▒███  ▒     ▒███  ▒███  ▒▒█████  ▒███    ███  ▒███  ▒███  ▒▒█████ ▒▒███  ▒▒███         ▒███  ▒▒█████  ▒███  ▒███      ▒███ ▒▒███     ███     
       █████       █████ █████  ▒▒█████ ██████████   █████ █████  ▒▒█████ ▒▒█████████        █████  ▒▒█████ █████ █████     █████ ▒▒▒███████▒      
      ▒▒▒▒▒       ▒▒▒▒▒ ▒▒▒▒▒    ▒▒▒▒▒ ▒▒▒▒▒▒▒▒▒▒   ▒▒▒▒▒ ▒▒▒▒▒    ▒▒▒▒▒   ▒▒▒▒▒▒▒▒▒        ▒▒▒▒▒    ▒▒▒▒▒ ▒▒▒▒▒ ▒▒▒▒▒     ▒▒▒▒▒    ▒▒▒▒▒▒▒        ";

        public static string bye = @"

                                                                       ╭───────────────────────────╮
                                                                       |   ┏┓ ╻ ╻┏━╸   ┏┓ ╻ ╻┏━╸╻  |
                                                                       |   ┣┻┓┗┳┛┣╸    ┣┻┓┗┳┛┣╸ ╹  |
                                                                       ╰   ┗━┛ ╹ ┗━╸   ┗━┛ ╹ ┗━╸╹ ノ
                                                                         ✓ ──────────────────────
                                                                ´   `
                                                              ૮(,╯︵╰,)𑁬
                                                               ( っ ς )
                                                                ˋO Oˊ 


                     ";

        public static string history = @"

                                                         ╭─────────────────────────────────────────────────────╮
                                                         | Why choose Log when you don't even want to open it? | 
                                                          ✓ ────────────────────────────────────────────────── ´                     
                                                 ( ಠ ⌓ ಠ ) ᴮʳᵘʰ




";

        public static string invalidage = @"
                                                                          ╭─────────────╮
                                                                          | Sorry hehe~ | 
                                                                           ✓ ────────── ´   
                                                                  (シ. .)シ                   ";

        public static string[] wakesup = { @" 
                                                                            v_v     z 
                                                                           (-.-)  Z   ", @" 
                                                                            v_v     Z
                                                                           (-.-) .   ", @" 
                                                                            v_v   
                                                                           (-.-) ", @"
                                                                          (\__/)   !!  
                                                                          (ㆆ_ㆆ)     ", @"
                                                                          (\__/)   ??  
                                                                          (•ᴖ •｡) " };

        public static string welcome = @"
 /$$      /$$ /$$$$$$$$ /$$        /$$$$$$   /$$$$$$  /$$      /$$ /$$$$$$$$       /$$$$$$$$ /$$$$$$       
| $$  /$ | $$| $$_____/| $$       /$$__  $$ /$$__  $$| $$$    /$$$| $$_____/      |__  $$__//$$__  $$  
| $$ /$$$| $$| $$      | $$      | $$  \__/| $$  \ $$| $$$$  /$$$$| $$               | $$  | $$  \ $$      
| $$/$$ $$ $$| $$$$$   | $$      | $$      | $$  | $$| $$ $$/$$ $$| $$$$$            | $$  | $$  | $$       
| $$$$_  $$$$| $$__/   | $$      | $$      | $$  | $$| $$  $$$| $$| $$__/            | $$  | $$  | $$         
| $$$/ \  $$$| $$      | $$      | $$    $$| $$  | $$| $$\  $ | $$| $$               | $$  | $$  | $$              
| $$/   \  $$| $$$$$$$$| $$$$$$$$|  $$$$$$/|  $$$$$$/| $$ \/  | $$| $$$$$$$$         | $$  |  $$$$$$/         
|__/     \__/|________/|________/ \______/  \______/ |__/     |__/|________/         |__/   \______/          
                                                                                                                                                                                                            
                                                                           HAVE  FUN!                                                    

                                          /$$ /$$$$$$$$ /$$                 /$$ /$$                           /$$   /$$ /$$                        /$$ /$$
                                          | $/| $$_____/|__/                | $$|__/                          | $$$ | $$|__/                       | $/| $$
                                          |_/ | $$       /$$ /$$$$$$$   /$$$$$$$ /$$ /$$$$$$$   /$$$$$$       | $$$$| $$ /$$ /$$$$$$/$$$$   /$$$$$$|_/ | $$
                                              | $$$$$   | $$| $$__  $$ /$$__  $$| $$| $$__  $$ /$$__  $$      | $$ $$ $$| $$| $$_  $$_  $$ /$$__  $$   | $$
                                              | $$__/   | $$| $$  \ $$| $$  | $$| $$| $$  \ $$| $$  \ $$      | $$  $$$$| $$| $$ \ $$ \ $$| $$  \ $$   |__/
                                              | $$      | $$| $$  | $$| $$  | $$| $$| $$  | $$| $$  | $$      | $$\  $$$| $$| $$ | $$ | $$| $$  | $$ 
                                              | $$      | $$| $$  | $$|  $$$$$$$| $$| $$  | $$|  $$$$$$$      | $$ \  $$| $$| $$ | $$ | $$|  $$$$$$/    /$$
                                              |__/      |__/|__/  |__/ \_______/|__/|__/  |__/ \____  $$      |__/  \__/|__/|__/ |__/ |__/ \______/    |__/
                                                                                               /$$  \ $$     
                                                                                              |  $$$$$$/   
                                                                                               \______/ ";

        public static string[] confused = { @" 
                                                                            v_v       
                                                                           (/_-;)    
                                                                           (   )\          
                                                                       _____T T______  ", @" 
                                                                            (\ /)      
                                                                            (O^O) ?!    
                                                                           (   )\\          
                                                                       _____T T______  ", @" 
                                                                          (\ /)      
                                                                      ?!  (O^O)     
                                                                         //(   )          
                                                                       _____T T______  ", @"
                                                                            (\ /)      
                                                                            (O^O) ?!    
                                                                           (   )\\          
                                                                       _____T T______  ", @" 
                                                                          (\ /)      
                                                                      ?!  (O^O)     
                                                                         //(   )          
                                                                       _____T T______  " };

        public static string[] start = { @" 
                                                                                                            (\ /)                                         
                                                                                                            (._.)                                     
                                                                                                           /(   )\                                               
                                                                                                      ───────T T─────────────────────────────────", @" 
                                                                                                              /) /)                                      
                                                                                                             ( o_o)                                  
                                                                                                            (\  )\                                                      
                                                                                                      ────── ┛  \────────────────────────────────", @" 
                                                                                                                  /) /)                             
                                                                                                                 ( O.O)                                     
                                                                                                                (\  )\                              
                                                                                                      ───────── ┘  ┓ ────────────────────────────", @" 
                                                                                                                          /) /)                             
                                                                                                                         ( ^.^)                                     
                                                                                                                         /   )\                 
                                                                                                      ─────────────────  ┛  \────────────────────", @" 
                                                                                                                                    /) /)           
                                                                                                                                   ( ^.^)           
                                                                                                                                  (\  )\           
                                                                                                      ───────────────────────────    ┓ ──────────" };

        public static string[] startwalk = { @" 
                                                                                                                           /) /)       
                                                                                                                          ( •.•)                
                                                                                                                          /   )\           
                                                                                                      -------------------  ┛  \------------------", @" 
                                                                                                                           /) /)       
                                                                                                                          ( ._.)    
                                                                                                                         /   )\           
                                                                                                      ------------------- ┘  ┓-------------------" };

        public static string[] STRAIGHT = { @" 
                                                                       /) /)                
                                                                      ( .⤙.)                    
                                                                      /   )\                        
                                                                 ---- ┛  \ ------  ", @" 
                                                                            /) /)               
                                                                           ( •^•)               
                                                                           /   )\                   
                                                                      ----- ┘  ┓ -----  ", @" 
                                                                                  /) /)      
                                                                                 ( •ᴗ•)    
                                                                                 /   )\          
                                                                           ----- ┘  ┓ -----  " };

        public static string[] JUMP = { @" 

                                                         /) /)     
                                                        (˶°ㅁ°)/ !!
                                                        /    )          
                                                  ────── /  \─────  ", @" 
 
                                                         /) /)      
                                                       \( >֊ <)/     
                                                        / ) / )          
                                                  ────────────────  " };

        public static string[] UP = { @" 
                                                                                      /) /)      
                                                                                    \( >ᗜ<)/     
                                                                                     / ) / )          
                                                                                ────────────────  
                                                                                ────────────────  ", @" 
                                                                                      /) /)      
                                                                                    \( >ᗜ<)/     
                                                                                     (    )          
                                                                                ──────/ ─\──────  
                                                                                ────────────────  ", @" 
                                                                                        /) /)      
                                                                                       („•∇•)    
                                                                                       (\   )\          
                                                                                  ───── ┛  \──────  
                                                                                ────────────────  ", @" 
                                                                                           /) /)      
                                                                                          („•∇•)/    
                                                                                           /   )          
                                                                                     ───── ┘  ┓──────  
                                                                                ────────────────  " };

        public static string[] DOWN = { @" 

                                                                                      /) /)     
                                                                                    \( >ᗜ<)/          
                                                                                ─────/ ) / )────  
                                                                                ────────────────  ", @" 

                                                                                      /) /)     
                                                                                    \( >ᗜ<)/          
                                                                                ─────(    )─────  
                                                                                ──────/ ─\──────  ", @" 

                                                                                          /) /)      
                                                                                         („•∇•)    
                                                                                    ─────(\   )\────      
                                                                                ──────-─── ┛  \──  ", @" 

                                                                                              /) /)      
                                                                                             („•∇•)     
                                                                                    ──────── /   ) ────         
                                                                                ──────────── ┘  ┓ ──  " };

        public static string[] TIRED = { @" 
                                                                             /) /)     
                                                                            (;´-`) =3       
                                                                            /   )\  
                                                                      ------ ┛  \------  ", @" 
                                                                                       /) /)      *sigh 
                                                                                      (ಥ⌓ಥ)    =3      
                                                                                      /   )\  
                                                                              -------- ┘  ┓----  " };

        public static string[] IGNORE = { @" 
                                                                         /) /)     
                                                                        (;/^-) =3     
                                                                            )\  
                                                                      - ┛  \-----------  ", @" 
                                                                            /) /)      
                                                                           (/-^-)         
                                                                              )\  
                                                                      ---- ┘  ┓--------  ", @" 
                                                                                /) /)     
                                                                               ( -^-) phew...     
                                                                              (\   )\  
                                                                      -------- ┛  \--- ", @" 
                                                                                    /) /)     
                                                                                   (ᵕ—ᴗ—) hehe..     
                                                                                  (\   )\  
                                                                      ------------ ┘  ┓ " };

        public static string[] SURE = { @" 
                                                                          ˚ /ᐠ ﮩﮩﮩ    
                                                                           / ꒱    マ ₊    
                                                                        ‧  (; ╥ ^ ╥ )
                                                                            ───୨ৎ──    
                                                                          ( \   |  ꒱            
                                                                          |  ꒱  | |
                                                                            ᗣ    ᗣ               ", @"                
                                                                         ˚  /ᐠ ﮩﮩﮩ    
                                                                           / ꒱    マ !! ₊    
                                                                       ‧   (; O ^ O ) 
                                                                            ───୨ৎ──    
                                                                          ( \   |  ꒱            
                                                                          |  ꒱  | |
                                                                            ᗣ    ᗣ               ", @"                   
                                                                            /ᐠ ﮩﮩﮩ                 ⋆˚࿔
                                                                           / ꒱    マ ₊          ⋆˚࿔    ⋆˚࿔
                                                                        ‧  (  ⊙ ロ ⊙)          ⋆࿔ C[ ]   ⋆˚࿔
                                                                            ───୨ৎ──          ⋆˚   [ ]   ⋆˚࿔
                                                                     ˚    ( \   |  ꒱          ⋆˚࿔     ⋆˚࿔
                                                                          |  ꒱  | |               ⋆˚࿔
                                                                            ᗣ    ᗣ", @"                               
                                                                            /ᐠ ﮩﮩﮩ          ⋆˚࿔                        
                                                                           / ꒱    マ    ⋆˚࿔    ⋆˚࿔                        
                                                                        ‧  ( ˵•̀ □•́ ˵) ⋆࿔  C[ ]   ⋆˚࿔      <----            
                                                                            ───୨ৎ──    ⋆˚  [ ]  ⋆˚࿔   <---            
                                                                     ˚    ( \   |  ꒱   ⋆˚࿔     ⋆˚࿔            
                                                                          |  ꒱  | |       ⋆˚࿔            
                                                                            ᗣ    ᗣ               ", @"                     
                                                                            /ᐠ ﮩﮩﮩ                                
                                                                           / ꒱    マ                            
                                                                        ‧  ( ๑>؂•̀๑ )                                     
                                                                            ─C[ ]>─                                       
                                                                     ˚    (   [ ]  ꒱                          
                                                                           \  ꒱   /                               
                                                                            ᗣ    ᗣ                   " };

        public static string[] STOP = { @"
                                                                     /) /)     
                                                                    (/◡̀_◡́)    
                                                                        )\  
                                                                 -- ┛  \---------------------  ", @" 
                                                                            /) /)      
                                                                           (ᵕ•ᴗ•)         
                                                                          (\   )\  
                                                                 --------- ┘  ┓--------------  ", @" 
                                                                                   /) /)     
                                                                                  ( ╹ -╹)?     
                                                                                  (\   )\  
                                                                 ------------------ ┛  \----- ", @" 
                                                                                       /) /)     
                                                                                      (｡•́︿•̀) ??     
                                                                                      /(   )\  
                                                                 ---------------------- | | -  " };

        public static string UNSURE = @"
                                                                          ˚ /ᐠ ﮩﮩﮩ    
                                                                           / ꒱    マ ₊    
                                                                        ‧  (; ¬ _ ¬')
                                                                            ───୨ৎ──    
                                                                          ( \   |  ꒱            
                                                                          |  ꒱  | |
                                                                            ᗣ    ᗣ         ";

        public static string OBSTACLE = @"
                         1____      2____      3____       
                          T  T       T  T       T  T                                                    .       . 
                          | `| 4____ | `| 5____ | `| 6____                                             /       /
                                T  T       T  T       T  T                                            /       /
                                | `|       | `|       | `|                                          _/_______/_
                                                                                                    ==========
                         \\    \\    \\    \\    \\    \\    \\                                    //UU/   //
                          \\          \\          \\          \\                                  //      //
                           \\    \\    \\    \\    \\    \\    \\                                //      //
                            \\          \\          \\          \\                              //      //
                             \\    \\    \\    \\    \\    \\    \\                            //      //
                              \\          \\          \\          \\                          //      //
                               \\    \\    \\    \\    \\    \\    \\                        //      //
                                \\                                  \\                      //      //
                                 \\          \\          \\          \\                    //      //
                                  \\                                  \\                  //      //
                                   \\                                  \\                //      //
                                    \\                                  \\              //      //







                                                                       |     < 0 >     |
                    ───────────────────────────────────────────────────|      you      |────────────────────────────────────────────────────";

        public static string DOOR = @"
                               1__________                   2___________                     3____________
                                T        T                    T         T                      T          T    
                                |        |                    |         |                      |          |
                                | [FOOD] |                    | [SLEEP] |                      | [GAMING] |
                                |      ◎ |                    |       ◎ |                      |        ◎ |
                                |        |                    |         |                      |          |
                                |        |                    |         |                      |          |
                                |        |                    |         |                      |          |
                                           4_______________               5__________________                6_______________
                                            T             T                T                T                 T             T    
                                            |             |                |                |                 |             | 
                                            | [SUBSTANCE] |                |                |                 |  [ SOCIAL]  |
                                            | [ADDICTION] |                | [ENTERTAINMENT]|                 |   [MEDIA]   |
                                            |             |                |                |                 | [PLATFORMS] | 
                                            |           ◎ |                |              ◎ |                 |           ◎ |
                                            |             |                |                |                 |             | 
                                            |             |                |                |                 |             | 
                                            |             |                |                |                 |             | 
                                            |             |                |                |                 |             | ";

        public static string DOOR_P = @"
──────────────────────────────────────────────────────────────────────── ( . ᗜ . ) ─────────────────────────────────────────────────────────────────────────
                                                                    ¿¿    V     V        ";

        public static string WALL = @"
                            ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـﮩ٨ـﮩﮩ٨ـ    
                            ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘
                            ║꒷꒦꒷꒦꒷꒦꒷꒦꒷꒦꒷𖦾_____𖦾            ꒷꒦꒷꒦꒷꒦꒷꒦꒷꒦꒷𒅒             ꒷꒦ ꒷꒦꒷꒷꒦   ꒷꒦꒷ ꒦ 
                            ║                      𖦾     𖦾                                                     𒇫                   ║
                            ║                      𖦾_____𖦾                    𒇫                                             𒇫 𒇫    ║
                            ║                       \\____\\                                                               𒇫       ║
                            ║                        𖦾     𖦾                                                                       ║
                            ║         𒇫           𖦾☰☰☰☰☰☰☰☰𖦾                        𒇫𒇫                                     ║
                            ║                         𖦾     𖦾                                           𒇫                          ║
                            ║   𒇫                     𖦾_____𖦾   𒅒                                                                  ║
                            ║             𒇫          //____//                                                                      ║
                            ║ ☰☰☰☰        𖦾☰☰☰☰☰☰𖦾                                                            𒅒          ║
                            ║                //____//                                     𒇫                               ☰☰☰   ║
                            ║☰☰☰𒄆 𒄆      𖦾      𖦾                                                                     𒄆 𒄆 𒄆     ║
                            ║    𒄆𒄆𒄆☰☰𒄆   𖦾_____𖦾                             𒇫                          ☰                      ║
                            ║𒄆 𒄆 𒄆 𒄆  𒄆 𒄆 𒄆𒄆☰☰☰☰                 𒇫 𒇫 ☰☰    ☰☰                  𒄆𒄆          𒄆 ☰ 𒄆 𒄆 𒄆      ║
                     vvvvVVVVVVVVVVVVVVVVVvvvvvvvvvvvvvvvvvvvvvvVVvvvvvvvvvvvvvvvvvvvvvvvVVVvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvVVV
                         //                                                                                                        \\
                        //                                                                                                          \\
                       //                                                                                                            \\
                      //                                                                                                              \\";

        public static string HAPPY = @" 
                                                                          ˚ /ᐠ ﮩﮩﮩ    
                                                                           / ꒱    マ ₊    
                                                                        ‧  (˶˃ ꒳ ˂˶)         
                                                                       ₍₍⚞  ───୨ৎ── ⚟⁾⁾     ";

        public static string DISTRICT = @" 
 .----------------. .----------------. .----------------. .----------------. .----------------. .----------------. .----------------. .----------------.   
| .--------------. ║ .--------------. ║ .--------------. ║ .--------------. ║ .--------------. ║ .--------------. ║ .--------------. ║ .--------------. |  
| |  ________    | ║ |     _____    | ║ |    _______   | ║ |  _________   | ║ |  _______     | ║ |     _____    | ║ |     ______   | ║ |  _________   | |  
| | |_   ___ `.  | ║ |    |_   _|   | ║ |   /  ___  |  | ║ | |  _   _  |  | ║ | |_   __ \    | ║ |    |_   _|   | ║ |   .' ___  |  | ║ | |  _   _  |  | |  
| |   | |   `. \ | ║ |      | |     | ║ |  |  (__ \_|  | ║ | |_/ | | \_|  | ║ |   | |__) |   | ║ |      | |     | ║ |  / .'   \_|  | ║ | |_/ | | \_|  | |  
| |   | |    | | | ║ |      | |     | ║ |   '.___`-.   | ║ |     | |      | ║ |   |  __ /    | ║ |      | |     | ║ |  | |         | ║ |     | |      | |  
| |  _| |___.' / | ║ |     _| |_    | ║ |  |`\____) |  | ║ |    _| |_     | ║ |  _| |  \ \_  | ║ |     _| |_    | ║ |  \ `.___.'\  | ║ |    _| |_     | |  
| | |________.'  | ║ |    |_____|   | ║ |  |_______.'  | ║ |   |_____|    | ║ | |____| |___| | ║ |    |_____|   | ║ |   `._____.'  | ║ |   |_____|    | |  
| |              | ║ |              | ║ |              | ║ |              | ║ |              | ║ |              | ║ |              | ║ |              | |  
| '--------------' ║ '--------------' ║ '--------------' ║ '--------------' ║ '--------------' ║ '--------------' ║ '--------------' ║ '--------------' |  
 '----------------' '----------------' '----------------' '----------------' '----------------' '----------------' '----------------' '----------------' 
                                                                          ▓█████▄ ▓█████   ██████  ██▓ ██▀███  ▓█████   ██████
                                      ██████  ███████                     ▒██▀ ██▌▓█   ▀ ▒██    ▒ ▓██▒▓██ ▒ ██▒▓█   ▀ ▒██    ▒
                                     ██    ██ ██                          ░██   █▌▒███   ░ ▓██▄   ▒██▒▓██ ░▄█ ▒▒███   ░ ▓██▄ 
                                     ██    ██ █████                       ░▓█▄   ▌▒▓█  ▄   ▒   ██▒░██░▒██▀▀█▄  ▒▓█  ▄   ▒   ██▒
                                     ██    ██ ██                          ░▒████▓ ░▒████▒▒██████▒▒░██░░██▓ ▒██▒░▒████▒▒██████▒▒
                                      ██████  ██                           ▒▒▓  ▒ ░░ ▒░ ░▒ ▒▓▒ ▒ ░░▓  ░ ▒▓ ░▒▓░░░ ▒░ ░▒ ▒▓▒ ▒ ░
                                                                              ░ ▒  ▒  ░ ░  ░░ ░▒  ░ ░ ▒ ░  ░▒ ░ ▒░ ░ ░  ░░ ░▒  ░ ░
                                                                              ░ ░  ░    ░   ░  ░  ░   ▒ ░  ░░   ░    ░   ░  ░  ░  
                                                                              ░      ░  ░      ░  ░     ░        ░  ░      ░  
                                                                              ░                                                         ";

        public static string[] OPENDOOR = { @"
                                           _______________
                                           T             T 
                                           |             |
                                           |             |
                                           |             |
                                           |   (\  /)    |
                                           |   (    )  ◎ |
                                           |  /(  ੭ )\   |
                                           |    /  \     |          ", @"
                                           _______________
                                           T             T 
                                           |             |
                                           |             |
                                           |             |
                                           |   /)  /)    |
                                           |   (  ╹-) /◎ |
                                           |  /( ੭  )    |
                                           |    |  \     |          ", @"
                                           _______________
                             - - - - - - - ║             T 
                             |             ║             |
                             |             ║             |
                             |             ║          !! |
                             |             ║    /)  /)   |
                             |  ◎          ║   (  >ㅁ)/  |
                             |             ║  /( ੭  )    |
                             |             ║    /  |     |          " };

        public static string[] CHEF = { @"

                                           ─────────
                                           ║███████║
                                           ║███████║
                                           ║███████║
                           ─────────────[█████████████]─────────────
                           ─────────────────────────────────────────", @"
                                           ─────────
                                           ║███████║
                                           ║███████║
                                           ║███████║
                                        [█████████████]
                           ──────────ooO──( =o _ o= )──Ooo──────────
                           ─────────────────────────────────────────", @"
                                           ─────────
                                           ║███████║
                                           ║███████║        Hello Traveler!
                                           ║███████║            Want some pie?
                                    ꉂ  [█████████████]
                           ──────────ooO──( =˃ ᗜ ˂= )──Ooo──────────
                           ─────────────────────────────────────────" };

        public static string PIE = @" 
                                                                                             ( 
                                                                                                )
                                                                                         __..---..__
                                                                                     ,-='  /  |  \  `=-.
                                                                                    :--..___________..--;
                                                                                    \                   / 
                                                                                     \.,_____________,./";

        public static string ERROR = @" 
                                            ░▒▓████████▓▒░▒▓███████▓▒░░▒▓███████▓▒░ ░▒▓██████▓▒░░▒▓███████▓▒░  
                                            ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ 
                                            ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ 
                                            ░▒▓██████▓▒░ ░▒▓███████▓▒░░▒▓███████▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓███████▓▒░  
                                            ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ 
                                            ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ 
                                            ░▒▓████████▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░░▒▓██████▓▒░░▒▓█▓▒░░▒▓█▓▒░";

        public static string WELCOME1 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░░░       ░░░        ░░░      ░░░        ░░       ░░░        ░░░░░░░░░      ░░░        ░░░░░░░░        ░░░      ░░░░      ░░░       ░░
        ▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒
        ▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓      ▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓▓▓▓▓▓      ▓▓▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓
        ███   ██   ██  ██████████████  █████  █████  ██  ████  ██████████████  ████  ██  ██████████████  ████████  ████  █  █████  ██  ███  ███
        ███       ███        ███      ███        ██  ████  ██        █████████      ███  ██████████████  █████████      ████      ███       ███
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string WELCOME2 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░░       ░░░        ░░░      ░░░        ░░       ░░░        ░░░░░░      ░░░        ░░░░      ░░░  ░░░░░░░░        ░░        ░░       ░░
        ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒
        ▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓      ▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓▓      ▓▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓      ▓▓▓▓       ▓▓
        ██   ██   ██  ██████████████  █████  █████  ██  ████  ███████████  ████  ██  ███████████████  ██  ████████  ████████  ████████  ███████
        ██       ███        ███      ███        ██  ████  ██        ██████      ███  ██████████      ███        ██        ██        ██  ███████
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string WELCOME3 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░░░       ░░░        ░░░      ░░░        ░░       ░░░        ░░░░░░░░      ░░░        ░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░
        ▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒
        ▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓      ▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓
        ███   ██   ██  ██████████████  █████  █████  ██  ████  █████████████  ████  ██  ██████████████  ████  ██        ██  █  █  ██  █████████
        ███       ███        ███      ███        ██  ████  ██        ████████      ███  ███████████████      ███  ████  ██  ████  ██        ███
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string WELCOME4 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░░░░░░░░░░░░░░░░░░░░░░░░░░       ░░░        ░░░      ░░░        ░░       ░░░        ░░░░░░░░      ░░░        ░░░░░░░░░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓      ▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  
        ██████████████████████████   ██   ██  ██████████████  █████  █████  ██  ████  █████████████  ████  ██  ████████████████████████████████ 
        ██████████████████████████       ███        ███      ███        ██  ████  ██        ████████      ███  ████████████████████████████████
        ░░░░░░░░░░░░░░░░░░░░░░░░      ░░░  ░░░░  ░░       ░░░░      ░░░        ░░░      ░░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓      ▓▓▓  ▓▓▓▓  ▓▓       ▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        █████████████████████████████  ██  ████  ██  ████  ████████  █████  █████        ██  ██    ██  ████  ██  ██████████████████████████████
        ████████████████████████      ████      ███       ████      ██████  █████  ████  ██  ███   ███      ███        ████████████████████████                               
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string WELCOME5 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░░░░░░░░░░░░░░░░░░░░░░░░░░       ░░░        ░░░      ░░░        ░░       ░░░        ░░░░░░░░      ░░░        ░░░░░░░░░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓      ▓▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  
        ██████████████████████████   ██   ██  ██████████████  █████  █████  ██  ████  █████████████  ████  ██  ████████████████████████████████ 
        ██████████████████████████       ███        ███      ███        ██  ████  ██        ████████      ███  ████████████████████████████████
        ░░░        ░░   ░░░  ░░        ░░        ░░       ░░░        ░░░      ░░░        ░░   ░░░  ░░  ░░░░  ░░        ░░   ░░░  ░░        ░░░░
        ▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒▒▒▒  ▒▒▒▒▒▒▒
        ▓▓▓      ▓▓▓▓  ▓  ▓  ▓▓▓▓▓  ▓▓▓▓▓      ▓▓▓▓       ▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓        ▓▓      ▓▓▓▓  ▓  ▓  ▓▓▓▓▓  ▓▓▓▓▓▓▓
        ███  ████████  ██    █████  █████  ████████  ███  ██████  █████        █████  █████  ██    ██  █  █  ██  ████████  ██    █████  ███████
        ███        ██  ███   █████  █████        ██  ████  █████  █████  ████  ██        ██  ███   ██  ████  ██        ██  ███   █████  ███████                               
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string WELCOME6 = @" 
        ░  ░░░░  ░░        ░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░        ░░░░░░░░        ░░░      ░░░░░░░░░        ░░  ░░░░  ░░        ░░░░
        ▒  ▒  ▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒
        ▓        ▓▓      ▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓▓▓▓  ▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓        ▓▓      ▓▓▓▓▓▓
        █   ██   ██  ████████  ████████  ████  ██  ████  ██  █  █  ██  █████████████████  █████  ████  ███████████  █████  ████  ██  ██████████
        █  ████  ██        ██        ███      ████      ███  ████  ██        ███████████  ██████      ████████████  █████  ████  ██        ████
        ░░       ░░        ░░      ░░        ░       ░░        ░░░░░      ░░░        ░░░░      ░░      ░░░░      ░░  ░░░░  ░░        ░       ░░
        ▒  ▒▒▒▒  ▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒  ▒  ▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒  ▒▒▒▒▒▒▒  ▒▒▒▒  ▒
        ▓  ▓▓▓▓  ▓      ▓▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓       ▓▓      ▓▓▓▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓▓      ▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓        ▓▓      ▓▓▓  ▓▓▓▓  ▓
        █   ██   █  █████████████  █████  ████  ██  ███  ██████████  ████  ██  ██████████████ █  ████  ██  ████  ██  █  █  ██  ███████  ████  █
        █       ██        ██      ███        █  ████  █        █████      ███  █████████      ██      ████      ███  ████  ██        █       ██
        ░░░░░░░░░░░░░░░░░░░░        ░░   ░░░  ░░       ░░░  ░░░░  ░░  ░░░░░░░░░      ░░░        ░░   ░░░  ░░░      ░░░        ░░░░░░░░░░░░░░░░░
        ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓   ▓▓      ▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
        ███████████████████████  █████  ██    ██  ████  ██  ████  ██  ████████  ████  ██  ████████  ██    ██  ████  ██  ███████████████████████
        ████████████████████        ██  ███   ██       ████      ███        ███      ███        ██  ███   ███      ███        █████████████████";

        public static string[] KEEPER = { @"
                        .:`:                          z  :`::`:.
                      . `  `.              .: .     Z      :: :: :
                      ::     ;             . .. . z           :  ::	 
                   .: `       :: :: :: :: ::    `: .           :: ::
                  . :                          ..  `            .. ..
                  .:                            `:               :: ::
                 ,                                `:: :: :: :: :: ::  ` :.
                 ;                                .              `; .    . .. .
                 :                                ::                 :: :` `` ::	
                 ` :      ────    ^   ────       ::                   :: .: :: :``	
                   ` .                             : :           . .. .: ::`` :
                .: :` `: ..                      .: ::  . .. .: :` `` `
                :: .. :: `` ``      ::.::.:. . .. ..	                            ", @"
                         .:`:                           :`::`:.
                      . `  `.              .: .           :: :: :
                      ::     ;             . .. . !!          :  ::	 
                   .: `       :: :: :: :: ::    `: .           :: ::
                  . :                          ..  `            .. ..
                  .:                            `:               :: ::
                 ,                                `:: :: :: :: :: ::  ` :.
                 ;        +==+        +==+         .              `; .    . .. .
                 :         〇          〇         ::                 :: :` `` ::	
                 ` :  ˵   `==`    ᗣ   `==`   ˵   ::                   :: .: :: :``	
                   ` .                             : :           . .. .: ::`` :
                .: :` `: ..                      .: ::  . .. .: :` `` `
                :: .. :: `` ``      ::.::.:. . .. ..	                            ", @"
                          .:`:                           :`::`:.
                      . `  `.              .: .           :: :: :
                      ::     ;             . .. .             :  ::	 
                   .: `    ᵕ  :: :: :: :: ::    `: .           :: ::    UHHHHH--- 
                  . :                     ᵕ    ..  `            .. ..       Want Some Bed?     
                  .:                            `:               :: ::
                 ,    ᵕ         ᵕ                `:: :: :: :: :: ::  ` :.
                 ;                  ᵕ           ᵕ  .              `; .    . .. .
                 :  ⸝⸝⸝⸝⸝⸝⸝⸝ ⚞⸝⸝⸝⸝⸝⸝⸝⸝⸝⸝⚟ ⸝⸝⸝⸝⸝⸝   ::                 :: :` `` ::	
                 ` :              ᗣ              ::                   :: .: :: :``	
                   ` .   ᵕ                         : :           . .. .: ::`` :
                .: :` `: ..                  ᵕ   .: ::  . .. .: :` `` `
                :: .. :: `` ``      ::.::.:. . .. ..	                            " };

        public static string BED = @" 
                                                                                    o──────O─────o
                                                                                   ||      °    ||
                                                                                   ||===\\  /===||
                                                                                   | ────\\/─────   
                                                                                   (             \
                                                                                  ||\             \      
                                                                                     \   ────────────
                                                                                      \ |            |
                                                                                       \|            |
                                                                                       ||───────────||";

        public static string[] GAMER = { @"
                           __________________
                           T                T
                           |        /       |...../)
                           |      (  )      |  ____) HEH.
                           |                |--|* |=		*yawns
                           |────────────────| 0    )	
                                  |  |    \     /___  *click *click
                              ───/____\===oOo==oOo===──────────
                           ─────────────────────────────────────────", @"
                           __________________
                           T                T
                           |        /       |...../)
                           |      (  )      |  ____) !!
                           |                |--| *|=		
                           |────────────────| ^    )	Hm? Why is so bright?
                                  |  |    \     /___  
                              ───/____\===oOo==oOo===──────────
                           ─────────────────────────────────────────", @"
                           __________________
                           T                T
                           |        /       |...../)
                           |      (  )      |  ____) !!
                           |                |--| *|=		
                           |────────────────| ^    )	Oh! AH- Want to borrow my console??
                                  |  |    \     /___  
                              ───/____\===oOo==oOo===──────────
                           ─────────────────────────────────────────" };

        public static string CONSOLE = @" 
                                                                                =====_                               _=====_
                                                                              / _____ \                             / _____ \
                                                                             +.-'_____'-.---------------------------.-'_____'-.+
                                                                            /   |     |  '.        S O N Y        .'  |  _  |   \
                                                                           / ___| /|\ |___ \                     / ___| /_\ |___ \
                                                                          / |      |      | ;  __           _   ; | _         _ | ;
                                                                          | | <---   ---> | | |__|         |_:> | ||_|       (_)| |
                                                                          | |___   |   ___| ;SELECT       START ; |___       ___| ;
                                                                          |\    | \|/ |    /  _     ___      _   \    | (X) |    /|
                                                                          | \   |_____|  .','' '', |___|  ,'' '', '.  |_____|  .' |
                                                                          |  '-.______.-' /       \ANALOG/       \  '-._____.-'   |
                                                                          |               |       |------|       |                |
                                                                          |              /\       /      \       /\               |
                                                                          |             /  '.___.'        '.___.'  \              |
                                                                           \           /                            \            /
                                                                            \_________/                              \__________/       ";

        public static string[] STRANGER = { @"
                                               ___    (    ___
                                              // \\  ---)- / \\
                                              (   ==  o  ==    )     *hufff
                                              ( ⸝⸝//     //⸝⸝  )	
                                   ||\\         {   _    _  }	
                              ____|  | \\       _| | |==| | |_
                             =    |__|  \\    -----|_|--|_|-----  .,.,.
                           ──────────────────────────────────────────────", @"
                                               ___      __  ___
                                              // \\ -- / / // \\
                                              (   ==   0/     )     *gulp *gulp
                                              ( ⸝⸝//     //⸝⸝ )	
                                   ||\\         {   _    _  }	
                              ____|  | \\       _| | |==| | |_
                             =    |__|  \\    -----|_|--|_|-----  .,.,.
                           ──────────────────────────────────────────────", @"
                                               ___         ___
                                              // \\  ----- / \\
                                              (   .   o   .    )   Oh??
                                              ( ⸝⸝//     //⸝⸝  )	        Want some cigar?
                                   ||\\         {   _    _  }	
                              ____|  | \\       _| | |==| | |_
                             =    |__|  \\    -----|_|--|_|-----  .,.,.
                           ───────────────────-|_____|--|_____|-──────────────────────" };

        public static string CIGAR = @" 
                                                                                                 (  )/ 
                                                                                                  )(/
                                                                              ________________   ( /)
                                                                             ()__)____________)))))     ";

        public static string[] HOST = { @"
                                                  -|─────|-
                                                    //|\\       *MUSIC
                                        *DRUMS

                                                   /\wvw/\
                                          EYY!   \ {> v <}    =3
                                                  V (    \\)>+
                                                     . - .	
                                    - - - - - - - - - - - - - - - - - - -", @"
                                                  -|─────|-
                                                    //|\\       *MUSIC
                                        *DRUMS

                                                   /\wvw/\
                                                   {o ` o} !    Hm?
                                              `+<( \    )\
                                                   . - .	
                                    - - - - - - - - - - - - - - - - - - -", @"
                                                  -|─────|-
                                                    //|\\       *MUSIC
                                        *DRUMS

                                                   /\wvw/\
                                                 \ {^ 0 ^} /    Hello new friend!!
                                               `+<(      )          Here have some drink!
                                                    . - .	
                                    - - - - - - - - - - - - - - - - - - -" };

        public static string DRINK = @" 
                                                                                          . 
                                                                                       |^ .
                                                                                      \O___.____ /
                                                                                        \   .  /     
                                                                                          \ ,/
                                                                                           []
                                                                                           []
                                                                                           []
                                                                                        --------";

        public static string[] SPECIALIST = { @"
                                                 {\-------/}
                                                 [ .◝ ꒳ ◜. ]      Hehe~
                                                     \|/ 
                                                 \  |---|  /
                                            ──────\_3   E_/──────	
                                                    |---|
                                    - - - - - - - - - - - - - - - - - - -", @"
                                                 {\-------/}
                                                 [ .°ㅁ° . ]     !!!
                                                     \|/ 
                                                 \  |---|  /
                                            ──────\_3   E_/──────	
                                                    |---|
                                    - - - - - - - - - - - - - - - - - - -", @"
                                                 {\-------/}
                                                 [ .^ ` ^. ]     Hello there!!
                                                     \|/           I see you have no phone,
                                                 \  |---|  /                here, have mine!
                                            ──────\_3   E_/──────	
                                                    |---|
                                    - - - - - - - - - - - - - - - - - - -" };

        public static string PHONE = @" 
                                                                                    ⢠⣾⣿⣿⠿⠿⠿⠿⠿⢿⣿⠿⢿⣿⣷
                                                                                    ⢸⣿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⣿⡇⠀
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇
                                                                                    ⢸⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇
                                                                                    ⢸⣿⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣿⡇
                                                                                    ⠘⢿⣿⣿⣿⣦⣤⣤⣤⣤⣴⣿⣿⣿⡿⠃";

        public static string[] PRE_CLIMB = { @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                           |    ║         ║
                                                ║         ║
                                  /) /)         ║         ║
                                 (; `,)/^   𖦾___║         ║
                              ──{    \ 𒄆   𖦾𒄆𒄆☰║         ║
                               ────|𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                         /) /) /║         ║
                                        (; `.)/ ║         ║
                                        {   ---+║         ║
                                     _(    )𖦾___║         ║
                                     v 𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ──", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║     
                                            𖦾   ║         ║     
                                            𖦾___║         ║     
                                             \  ║         ║     
                                             𖦾\_║         ║     
                                           𖦾☰☰║         ║       
                                           /    ║         ║     
                                            |---║         ║     
                                            |   ║         ║     
                                         𖦾☰☰☰║         ║        
                                           𖦾    ║         ║     
                                           |    ║         ║     
                                                ║         ║     
                                                ║         ║     
                           /) /)            𖦾___║         ║     
                          ( .^.)       𒄆   𖦾𒄆𒄆☰║         ║      
                          ( \\  )_,,𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── " };

        public static string[] CLIMB = { @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                           |    ║         ║
                                                ║         ║
                                  /) /)         ║         ║
                                 (; `,)/^   𖦾___║         ║
                              ──{    \ 𒄆   𖦾𒄆𒄆☰║         ║
                               ────|𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                         /) /) /║         ║
                                        (; `.)/ ║         ║
                                        {   ---+║         ║
                                     _(    )𖦾___║         ║
                                     v 𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ──", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰☰☰║         ║
                                         𖦾/)/) /║         ║
                                   ...   (.-.)/ ║         ║
                                          (   )_║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @" 
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                           /    ║         ║
                                            |---║         ║
                                            |   ║         ║
                                         𖦾☰**☰║         ║
                                         𖦾 (.-.)║         ║
                                   Hmm     (v v)║         ║
                                            | | ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰/)║         ║
                                           /(. )/         ║
                                            -o_)║         ║
                                           ||  \║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾/)║         ║
                                          𖦾(. )o║         ║
                                           /(   )         ║
                                           |  ; ║         ║
                                           |    ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                       !!    𖦾/)║         ║
                                           (0 )o║         ║
                                         𖦾 /(   )         ║
                                        /     ; ║         ║
                                      \         ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                  (\ (\         ║         ║
                                 \( O.O)/       ║         ║
                                  ( /   )/      ║         ║
                                         𖦾☰**☰║         ║
                           #  .        ;  𖦾     ║         ║
                                     /    |     ║         ║
                              \                 ║         ║
                                                ║         ║
                            𖦾        ,      𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── " };

        public static string[] SUCCESS = { @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                           |    ║         ║
                                                ║         ║
                                  /) /)         ║         ║
                                 (; `,)/^   𖦾___║         ║
                              ──{    \ 𒄆   𖦾𒄆𒄆☰║         ║
                               ────|𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰☰☰║         ║
                                           𖦾    ║         ║
                                         /) /) /║         ║
                                        (; `.)/ ║         ║
                                        {   ---+║         ║
                                     _(    )𖦾___║         ║
                                     v 𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ──", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾P ☰☰║         ║
                                         𖦾/)/) /║         ║
                                         (`-`)/ ║         ║
                                          (   )_║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                         𖦾 (.-.)║         ║
                                           (v v)║         ║
                                            | | ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             𖦾\_║         ║
                                           p☰/)║         ║
                                          ||`^`)/         ║
                                         __(  _)║         ║
                                               \║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                             \  ║         ║
                                             P/)║         ║
                                          𖦾(. )o║         ║
                                           (   )^         ║
                                               \║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"


                                                ٨ـﮩ٨ـﮩ٨ـﮩﮩ٨     
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾**_║         ║
                                           (.-.)║         ║
                                           (v v)║         ║
                                          𖦾 | | ║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"

                                                    *POOF! 
                                         !!                  *POOF! 
                                           /) /)⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                           (`^`)/ ꒷꒦꒷꒦║
                                           (   )║         ║
                                           _|𖦾|_║         ║
                                               \║         ║
                                              𖦾\║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                     /) /)
                                                    ( ._)  Uhhhhh
                                                   /(   )\    Here goes nothing--
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘      
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                               \║         ║
                                              𖦾\║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                     
                                                                /) /) 
                                                                \(*A*)/  AAAAAAAAAAAAAAAAA
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘     ( ^^) 
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                               \║         ║
                                              𖦾\║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║
                                            𖦾___║         ║
                                       𒄆   𖦾𒄆𒄆☰║         ║
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                     
                                                                                                                    
                                                                                                                                
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘     
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                               \║         ║
                                              𖦾\║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║ *POOF!   (\ /)   ????
                                            𖦾___║         ║          (.-.)
                                       𒄆   𖦾𒄆𒄆☰║         ║        ~/(   )\~       *POOF!
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║──  ──(  ` `   ) ── ── ── ── ── ", @"
                                                     
                                                                 
                                                                
                                                ⫘⫘⫘⫘⫘⫘⫘⫘⫘⫘     
                                            𖦾___║ ꒷꒦꒷꒦║
                                            𖦾   ║         ║
                                            𖦾___║         ║
                                               \║         ║
                                              𖦾\║         ║
                                           𖦾☰☰║         ║
                                                ║         ║
                                                ║         ║
                                                ║         ║
                                         𖦾☰**☰║         ║
                                          𖦾     ║         ║
                                          |     ║         ║
                                                ║         ║
                                                ║         ║                          /) /)
                                            𖦾___║         ║                         (.v.)   waahh how magical!
                                       𒄆   𖦾𒄆𒄆☰║         ║        ~~~~~~~~        (\  )\       
                               ─────𒄆𒄆𒄆☰𒄆☰☰☰║─────────║──  ──(  ` `   ) ── ── ── \─ ── " };

        public static string[] FAIL = { @"
                                                                        ║         ║     
                                                    /) /)               ║         ║     
                                                   ( .^.)           𖦾___║         ║     
                                                 /(    )\      𒄆   𖦾𒄆𒄆☰║         ║      
                                                   |  |  _,,𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                                        ║         ║     
                                           (\ (\                        ║         ║     
                                           (•̀⤙•́ )    =3             𖦾___║         ║     
                                           /(    )   =3        𒄆   𖦾𒄆𒄆☰║         ║      
                                              /          _,,𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ──" };

        public static string[] AGAIN = { @"
                                                                        ║         ║     
                                                                        ║         ║     
                                                   /) /)            𖦾___║         ║     
                                                  ( .^.)       𒄆   𖦾𒄆𒄆☰║         ║      
                                                  ( \\  )_,,𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── ", @"
                                                                        ║         ║     
                                                   /) /)                ║         ║     
                                                  ( .^.)            𖦾___║         ║     
                                                 /(    )\      𒄆   𖦾𒄆𒄆☰║         ║      
                                                   |  |  _,,𒄆𒄆𒄆☰𒄆☰☰☰║─────────║── ── ── ── ── ── ── ── " };

        public static string PEOPLE = @"
                                                                  *\  /*            
                                                                   [  ]             ^...^
                                                                 /(    )|           (   )   
                                         __________________________d  d_____________[ v ]_________________________";

        public static string[] FRIEND = { @"
                       *\  /*    Hmm??
                   !!   [  ]  
                       +(   )+
                      /(    )|            
            ____________d  d____________", @"
                       *\  /*   
                  ??    [O.O]      *turns around  
                       +(   )+
                       |(    )\             
             ____________b  b___________ " };

        public static string FRIEND_HI = @"
                       *\  /*   
                  ??    [U^U]      Wahh~
                       +(/*\)+        <3  
                        (    )         <3       
            _____________b  b___________ ";

        public static string FRIEND_SAD = @"
                       *\  /*   
                       [.^.]    sniff
                      +(   )+          
                      |(   )|         
          ______________b  b__________ ";

        public static string[] DOG = { @"
                                                                                                                          !!    ^...^ 
                                                                                                                                (   )
                                                                                                                                {   }              
                                                                                                                    ____________[ v ]____________", @"
                                                                                                                          ??    ^...^ 
                                                                                                                                (o-o)     *turns around      
                                                                                                                                {   }              
                                                                                                                    ____________[d b]____________" };

        public static string DOG_HI = @"
                                                                                                                                ^...^ 
                                                                                                                                (^O^) /    <3
                                                                                                                                {    /       <3             
                                                                                                                    ____________[d  ]____________";

        public static string DOG_SAD = @"
                                                                                                                                ^...^ 
                                                                                                                                (.^.)    *whimpers
                                                                                                                                {   }                   
                                                                                                                     ___________[ `b]____________";

        public static string DOG_DIE = @"
                                                                                                                    _ _ _ _ _ ;[  | ]> _ _ _ _ ";

        public static string PEOPLE_HI = @"
                                                                     *\  /*            
                                                         Hello!      \[^v^] /         ^...^
                                                                     (    )           (^-^)   Awwooo!
                                            __________________________b b ____________[; ;]_________________________";

        public static string HI = @"
                                                                            /) /)    <3  
                                                                           ( ^o^)/ 
                                                                            /   )        <3  
                                                                ____________|  |___________ ";

        public static string LOW = @"
                                                                            /) /)   
                                                                           ( o^o)  
                                                                           /   )\       
                                                               ____________|  |___________ ";

        public static string[] FRIENDWALK = { @" 
                                                                 /) /)  *\\  /*
                                                                ( ^.^)   [^v^]/   ^...^
                                                                 /   )   /   )    (ovo)     
                                                            _ _ _  / _ _ _ `b _ _ c[\  \ _ _ _", @" 
                                                                 /) /)  *\\  /*
                                                                ( ^.^)   [^v^]     ^...^
                                                                \   )   \   )\    (ovo)	
                                                           _ _ _    \ _ _ `b _ _ _c[\  / _ _ _" };

        public static string DUO = @"
                                                                       /) /)          
                                                                      ( .^.)       ^...^
                                                                        /\)       (o^o)  
                                                      ________________ |  | _______ |  |]>______________";

        public static string HUG = @"
                                                                         /) /) 
                                                                        ( v^v)^...^    *hugs
                                                                        (  \\(ovo)  
                                                              __________  ) )_|  |]>________";

        public static string OLD = @"
                                                                             /) /)     
                                                                            (;/^.)  *sighh
                                                                             V  \,
                                                                           ,  ]  ]  ";

        public static string CHECK = @"                                                                               
                                                                                                                ___
                                                                        /) /)                                   | |!
                                                                       ( o.o)     ??                            | |  !
                                                                       (\   )\                                ! |_|
                                                                         ||                                     (_) !";

        public static string[] SAD = { @" 
                                                                             /) /)     
                                                                            ( /^\ )       
                                                                             /   \ 
                                                                           <(  ]  ]  ", @" 
                                                                             /) /)     
                                                                            ( /^\ ) 
                                                                             V /| 
                                                                           _]  |.  ", @" 
                                                                             /) /)     
                                                                            ( .^\ ) 
                                                                            (/   )
                                                                             |  |  ", @" 
                                                                             /) /)     
                                                                            ( .^.) 
                                                                            (\   )\
                                                                               \ ", @" 
                                                                                          /) /)     
                                                                                         ( .^.) 
                                                                                        (\   )\
                                                                                           \ " };

        public static string[] BURIAL = { @" 

                                                                             /) /)     
                                                                            ( ;.;)==*      
                                                                _ _ _ _ _ ,( ;[ \| ]>           _
", @" 

                                                                                 /) /)     
                                                                                ( ;.;)       
                                                                _ _ _ _ _    ,,( ;[ \| ]>  _ _ _
                                                                                       |________|", @" 

                                                                                       /) /)     
                                                                                      ( ToT)       
                                                                _ _ _ _ _           ,,(  ,_\ _ 
                                                                                       |[ \| ]>|", @" 

                                                                                       /) /)     
                                                                                      ( ToT)/--`@   
                                                                _ _ _ _ _           ,,(  ,_\ _ 
                                                                                       |[ \| ]>|", @" 

                                                                                       /) /)     
                                                                                      ( ToT)  
                                                                _ _ _ _ _           ,,(  ,_\--`@
                                                                                       |[ \| ]>|", @" 

                                                                                  /) /)     
                                                                                 ( /^\)
                                                                _ _ _ _ _     ,(  ) )_______--`@
                                                                                       |[ \| ]>|", @" 
                                                                                 /) /)   
                                                                                ( /^\) 
                                                                                (    )
                                                                _ _ _ _ _        |  | ______--`@
                                                                                       |[ \| ]>|" };

        public static string[] ADULT_SAD = { @" 
                                                                             /) /)     
                                                                            (;/~\)      
                                                                             /   \ 
                                                                           <(  ]  ]  ", @" 
                                                                             /) /)     
                                                                            (;/^.)  *sighh	  	
                                                                             V  \,
                                                                           ,  ]  ]  ", @" 
                                                                             /) /)     
                                                                            (;.^.) 
                                                                             \, /|    *pants
                                                                           ,  ]  L  ", @" 
                                                                             /) /)     
                                                                            (;.o.)  *pants
                                                                            (\   )\
                                                                               \ ", @" 
                                                                                          /) /)     
                                                                                         (;.o.)  *pants
                                                                                        (\   )\
                                                                                           \ " };

        public static string[] PRE_TRIAL3 = { @"
         /) /)     
        ( '^') 
        (\   )\
           \ 
", @" 
                              /) /)     
                             ( -^-) 
                            (\   )\
                              / 
", @" 
                                                  /) /)     
                                                 ( .^.)/ 
                                                 (\   )
                                                    \ 
", @"                                                                               
                                                                                                                ___
                                                                        /) /)                                   | |!
                                                                       ( o.o)/    ??                            | |  !
                                                                       (\   )                                 ! |_|
                                                                          /                                     (_) !" };

        public static string[] CONTINUE = { @"
         /) /)     
        ( '^') 
        (\   )\
           \ ", @" 
                              /) /)     
                             ( -^-) 
                            (\   )\
                              / ", @" 
                                                  /) /)     
                                                 ( .^.)/ 
                                                 (\   )
                                                    \ ", @" 
                                                                        /) /)     
                                                                       ( -o-)/     *sighh
                                                                       (\   )
                                                                          / ", @" 
                                                                                                      /) /)             
                                                                                                     ( -.-)     Hm...       
                                                                                                     (\   /                 
                                                                                                        \                   ", @" 
                                                                                                     (\ (\                  
                                                                                                     (o.o )     *sighh
                                                                                                      \  /)                        
                                                                                                       / ", @" 
                                                                       (\ (\                                                
                                                                       (o^o )     *sighh                                        
                                                                       (   \)                                               
                                                                         \                                                      ", @" 
                                                  (\ (\                                             
                                                  (o'o )                                    
                                                  (   /)                                            
                                                    /                                                   ", @" 
                              (\ (\                             
                             ( -^-)                             
                            (\   )\                                  
                              /                         ", @" 
                                                                        /) /)                                   !\          
                                                                       ( o.o)/    ??                            |/      
                                                                       (\   )                                   |           
                                                                          /                                  __[ ]__    ", @" 
                                                                        /) /)                                   !\          
                                                                       ( o.o)/    Ehhh?                         |/          
                                                                      /(    )\                                  |           
                                                                        /  \                                 __[ ]__            " };

        public static string[] ADULT = { @"
         /) /)     
        (;'^')  *pants
        (\   )\
           \ ", @" 
                              /) /)     
                             (\-.-)  *wipes sweat
                            (\   )\
                              / ", @" 
                                                  /) /)     
                                                 (;-^-)/  *wipes sweat 
                                                 (\   )
                                                    \ ", @" 
                                                                        /) /)     
                                                                       (;=0=)/   *huff *huff  
                                                                       (\   )
                                                                          / ", @" 
                                                                                                      /) /)                             
                                                                                                     (;-.-) Ouch my back...                 
                                                                                                     (\   /                     
                                                                                                        \ ", @" 
                                                                                                     (\ (\                  
                                                                                                     (.^.;) 
                                                                                                      \  /)             
                                                                                                       /                ", @" 
                                                                       (\ (\                                                        
                                                                       (o^o;)  	*pants                                              
                                                                       (   \)                                                               
                                                                         \                                                          ", @" 
                                                  (\ (\                                                                             
                                                 (=-=;)  I sure am getting old..                                                
                                                  (   /)                                        
                                                    /                                               ", @" 
                              (\ (\     
                             ( -^-) 
                            (\   )\
                              / ", @" 
                                                                        /) /)                                   !\
                                                                       ( o.o)/    ??                            |/
                                                                       (\   )                                   |
                                                                          /                                  __[ ]__", @" 
                                                                        /) /)                                   !\
                                                                       ( o.o)/    Ehhh?                         |/
                                                                      /(    )\                                  |
                                                                        /  \                                 __[ ]__" };

        public static string GLITCH = @"
                                  ███████           ███                 ███████ ██ █                   ███ 
                ███          ███      ███████████   ███████       █████   ███                  ██████  
            ███   ███ ██████                      ███                     ██████████████████ ███         ████      ████
          █████        ██████████        ██████ ███               █████              ███  ██████ ███ █████████████                   ███ ████████ 
          █████████████████████████ ███████████  ███████████  ███      ██████        █████████ ████ ███████████████████  █████    ███        
        █ ████ ██████   ███    ███    ████        ████     ███ ███      ███ ███      ██  ████ █   █████ ███     ███ █████     ███ ██████   ███ 
        █ ████ ██████   ███    ███    ████        ████    ████ ███     ████ ███      ██  ████   ██████  ███     ███ ███       ███ ███████  ███     ██
          ████ ███ ████ ███    ███    ██████ ████ ███████████  ███████████  ███      ██  █████████████  ███     ███ ███       ███ ███ ████ ███  ███     
          ████ ███  ███████    ███    ████      ██████  ████   ███   ████   ███      ██  ███      ███   ███     ███ ███       ███ ████ ██████  ███  
        ██████████ █  ███████  ███   █████     ███████    ████ ███    ████  ████    ███ ████      ██    ███     ████ ████    █████████   █████ 
          ████ ███     ████    ███    ████████████████     ███ ███      ████ ██████████  ███     █████  ███   ██████  █████████   ████   ████████ 
        █     ███████     ███████     ██  ███            ████  ██                       ████   ████████   ██████     ████   █████████████████             
              ███████     ███████        ██        ████    █████ ██         █████        ████    ████████   ████    ███████ █████████████████ 
                           █████       █████       ████         ████  █████  ████        ████       ████    █████   ███████        ████          
                            ███       ██████████████████         ███ ████████████        ████      ███████   ███       █           ████                 
                    ████       ███████                    ███       ████ ██      █████        █████████████   ████████
                                ████                      ███       ████████       ███       
                                ████                     ████        ██████      █████
                                  █                         ███      ███████   ";

        public static string OBSTABLE2 = @"
███████████████████████████████████████████████████████████████████████     0     █████████████████████████████████████████████████████████████████████████";

        public static string SHORE = @"
        ___
        | |
========| |==:`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,
█████████████ ";

        public static string[] SAIL = { @"
                 |\
                 | \  (\ /)
                 |_/  (^v^)/
                 |   /(   )
        *whoosh  |    _| |
           =+==+==+==//=+==+==+==+=
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.", @"
                            /|
                           / |    (\ /)         Full speed ahead!
                           \_|   \(ovo)/
                             |    (   )
                             |    _| |
             *splash   =+==+==+==//=+==+==+==+=
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-", @"
                                        /|
                                       / |     (\ /)         Woohh!!
                                       \_|    \(^.^)/
                                         |    (   )
                                         |    _| |
                 *splash  *splash  =+==+==+==//=+==+==+==+=
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-", @"
                                                    /|
                                                   / |     (\ /)   !!
                                                   \_|     (o.o)
                                                     |   /(   )\
                                                     |    _| |
                             *splash  *splash  =+==+==+==//=+==+==+==+=
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-", @"
                                                                /|
                                                               / |     (\ /)  !!  
                                                               \_|     (oAo)        Oh no!! I'm heading towards the storm!
                                                                 |     (/ \)
                                                                 |    _ | |
                                         *splash  *splash  =+==+==+==//=+==+==+==+=
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-", @"
                                                                            /|                                                    *rumble   .+(`  )`..
                                                                           / | (\ /)  !!                                                :(   (   ))   -
                                                                           \_| ( o-o)                                               (.--..,___.--,--'
                                                                             |\-(    )                                              *rumble     *rumble 
                                                                             |   | |                                                 .'''-.  
                                                     *splash  *splash  =+==+==+==//=+==+==+==+=                                        .,       `.   '. 
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-", @"
                                                                                    /|                            *rumble   .+(`  )`....   ...
                                                                                   / |  (\ /)  HELP                   :(   (   ))   -.,   --- ..
                                                                                   \_| ( T-T)                       (.--..,___.--,--'   ---  . .
                                                                                     |\-(    )                          *rumble      *rumble 
                                                                                     |   | |                    .'''-.     .'''-.      .'''-.    .'''-.
                                                             *splash  *splash  =+==+==+==//=+==+==+==+=          '.    `.    '.   `.    '.    `. '.     
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.", @"
                                                                                        /|           *rumble   .+(`  )`....   ...             _.====.._ 
                                                                                       / | (\ /)  !!                   _.====.._           ,:._        
                                                                                       \_| ( QAQ)   AAAAAAAAAAAA    ,:._          ~-             `\\       
                                                                                         |\-(    )        NOOOOOOOO      \        ~-_             |       
                                                                                         |   | |                           | _  _     `.          /  _  
                                                                 *splash  *splash  =+==+==+==//=+==+==+==+= '.    `.     ,/ /_)/ | |    ~-      ,/ /_)/  
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.," };

        public static string[] WAVE = { @"
                                                                    _._       /|                    _._
                                                                  ,`' '`,    / |                  ,`' '`,
                                                                 .       .   \_| (\ /)  AAAAAAAA .       .
                                                                ;         ;    |\(OAO)          ;  	    ;     
                                                         .-.   .-.   .         |/(^,^),              -.   .-.     -.   .-.  
                                                            '-'   '-'    =+==+==+==//=+==+==+==+='-'   '-'   '-'   '-'
                                          )`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,", @"
                                                                    _._       /|                    _._
                                                                  ,`' '`,    / |                  ,`' '`,
                                                                 .       .   \_| (\ /) ??        .       .
                                                                ;         ;    |\(OAO)          ;  	    ;     
                                                         .-.   .-.   .         |/(^,^),              -.   .-.     -.   .-.  
                                                            '-'   '-'    =+==+==+==//=+==+==+==+='-'   '-'   '-'   '-'
                                          )`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,", @"
                                                                    _._       /|                    _._
                                                                  ,`' '`,    / |                  ,`' '`,
                                                                 .       .   \_|  (\ /)  EHHH?   .       .
                                                                ;         ;    |\ (*O*)         ;  	    ;     
                                                         .-.   .-.   .         |/(^,^),              -.   .-.     -.   .-.  
                                                            '-'   '-'    =+==+==+==//=+==+==+==+='-'   '-'   '-'   '-'
                                          )`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.," };

        public static string STORM = @"
  / / /       / / / / / / / *crack // / / / / /    /   / / / /         *crack /     /   /|                   (     )      --==  888(     ).=-- *roar (   
    / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / / / /  / / /  / / /  / |  (\ /)           `(    )  ))    )-    ((    (..__.:'-        
       / / / / / /               / / / / / /                                           \_|  ( `-`)        --..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..
  / / /       / / / / / / / *crack // / / / / /    /   / / / /         *crack /     /    |\-(    )     / / / / / / / / / / *crack // / / / / / / / / / / 
/ / / / / /  /   /    /      /   /   / /  / / / / /  /   // / / / / /  /   /    /        |   | |   / / / / / /  /   /    /      /   /   / /  / / / / /  /
   / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / *splash  *splash  =+==+==+==//=+==+==+==+= / / / / / /  /   /    /      /   /   / /  /  
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-., ";

        public static string[] STORMY = { @"
   ;     ____      ''         ;     __      ,    ;  .               ;    ______         ;     ____      ''         ;     __      ,    ;  .               ;
      .+(`  )`.  .    .         .+(`  )`.        _____      ;         .+(`  )` .         ________      .+(`  )`.  .    .         .+(`  )`.         _____ 
 .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )      --==  888(     ).=-- *roar (   (   )) .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )
 ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-    ((    (..__.:'-        `- __.' ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-   
--..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,--'`,---..-.--+-..,___.--,-----..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,
/ /     /       / rumble    /    / / / / / / /    /  / /   / /    / / / /      /   / / / / // /     /       / rumble    /    / / / / / / /    /  / /   / 
    / / /       / /     / / *crack // / / / / /    /   / / / /  *crack /     /          /|  /    / / / / / / /    /  / /   / /    / / / /      /   / / / 
    /    / /  /   /    /      /   /     /    / /    /  /    / / / /     /  / /  / / /  / |  (\ /) ??   / 	//       //   / / /     rumble   /  /       / 
       / /      / /    / /               /     /   /     /  /  /                       \_|  ( `-`) / / /      /   /  /      / / /        /
  /   / /       / /    / / / /   / /    / /    /   /     / / /         *crack /     /    |\-(    )   / / / / / / / / / / *crack // / / / / / / / / / / 
        / / / / / /   /      /   /   / /  / / / / /  /   // / / / / /  /   /    /        |   | |  / / / / / /  /   /    /      /   /   / /  / / / / /  /
   / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / *splash  *splash  =+==+==+==//=+==+==+==+= / / / / / /  /   /    /      /   /   / /  / 
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,    ", @" 
   ;     ____      ''         ;     __      ,    ;  .               ;    ______         ;     ____      ''         ;     __      ,    ;  .               ;
      .+(`  )`.  .    .         .+(`  )`.        _____      ;         .+(`  )` .         ________      .+(`  )`.  .    .         .+(`  )`.         _____ 
 .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )      --==  888(     ).=-- *roar (   (   )) .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )
 ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-    ((    (..__.:'-        `- __.' ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-   
--..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,--'`,---..-.--+-..,___.--,-----..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,
/ /     /       / rumble    /    / / / / / / /    /  / /   / /    / / / /      /   / / / / // /     /       / rumble    /    / / _.====.._    /  / /   / /
    / / /       / /     / / *crack // / / / / /    /   / / / /  *crack /     /          /|  /    / / / / / / /    /  / /   /   ,:._       ~-_  
    /    / /  /   /    /      /   /     /    / /    /  /    / / / /     /  / /  / / /  / |  (\ /)      / 	//       //   / /     \\           \\.      
       / /      / /    / /               /     /   /     /  /  /                       \_|  ( OAO) !! / / /      /   /  /      /     ; _  _  |      `.
  /   / /       / /    / / / /   / /    / /    /   /     / / /         *crack /     /    |\-(    )   / / / / / / / / / / *crack      ; _  _  |       `.
        / / / / / /   /      /   /   / /  / / / / /  /   // / / / / /  /   /    /        |   | |  / / / / / /  /   /    /      /      / | _  _         `.
   / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / *splash  *splash  =+==+==+==//=+==+==+==+= / / / / / /  /   /         ,/ /_)/ | |    ~-_ 
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-., ", @" 
   ;     ____      ''         ;     __      ,    ;  .               ;    ______         ;     ____      ''         ;     __      ,    ;  .               ;
      .+(`  )`.  .    .         .+(`  )`.        _____      ;         .+(`  )` .         ________      .+(`  )`.  .    .         .+(`  )`.         _____  
 .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )      --==  888(     ).=-- *roar (   (   )) .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )
 ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-    ((    (..__.:'-        `- __.' ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-   
--..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,--'`,---..-.--+-..,___.--,-----..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,
/ /     /       / rumble    /    / / / / / / /    /  / /   / /    / / / /      /   / / / / // /     /       / rumble  _.====.._     
    / / /       / /     / / *crack // / / / / /    /   / / / /  *crack /     /          /|  /    / / / / / / /   ,:._       ~-_  
    /    / /  /   /    /      /   /     /    / /    /  /    / / / /     /  / /  / / /  / |  (\ /)      / 	//        \\           \\.          
       / /      / /    / /               /     /   /     /  /  /                       \_|  ( TOT) NOT AGAIN!   /       ; _  _  |      `.
  /   / /       / /    / / / /   / /    / /    /   /     / / /         *crack /     /    |\-(    )   / / / / / / /       ; _  _  |       `.
        / / / / / /   /      /   /   / /  / / / / /  /   // / / / / /  /   /    /        |   | |  / / / / / /  /          | _  _         `.         
   / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / *splash  *splash  =+==+==+==//=+==+==+==+= / / / /      ,/ /_)/ | |    ~-_  
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-., ", @" 
   ;     ____      ''         ;     __      ,    ;  .               ;    ______         ;     ____      ''         ;     __      ,    ;  .               ; 
      .+(`  )`.  .    .         .+(`  )`.        _____      ;         .+(`  )` .         ________      .+(`  )`.  .    .         .+(`  )`.         _____  
 .+(`(      .   )   ; .--  `.  (    ) )    ;    (     )      --==  888(     ).=-- *roar (   (   )) .+(`(      .   )   ; .--  `.  (    ) )    ;    (     ) 
 ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-    ((    (..__.:'-        `- __.' ((    (..__.:'-'   .=(   )   ` _`  ) )   (    )  ))    )-   
--..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,--'`,---..-.--+-..,___.--,-----..,___.--,--'`,---..-.--+--.,,-,,..._.--..-..,.,.-..,___.--,-
/ /     /       / rumble    /    / / / / / / /    /  / /   / /    / / / /      /   / / / / // /       _.====.._     
    / / /       / /     / / *crack // / / / / /    /   / / / /  *crack /     /          /|  /    / ,:._         ~-_  
    /    / /  /   /    /      /   /     /    / /    /  /    / / / /     /  / /  / / /  / |  (\ /)      \\           \\.          
       / /      / /    / /               /     /   /     /  /  /                       \_|  ( >A<)  /    ; _  _  |      `.
  /   / /       / /    / / / /   / /    / /    /   /     / / /         *crack /     /    |\-(    )   / /  ; _  _  |       `.
        / / / / / /   /      /   /   / /  / / / / /  /   // / / / / /  /   /    /        |   | |  / / / /  | _  _         `.                     
   / / /   / / /  /   /    /      /   /   / /  /  / / / /  /   / *splash  *splash  =+==+==+==//=+==+==+==,/ /_)/ | |    ~-_  
`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-.,`'-.,_)`'-.,_)`'-., " };

        public static string CRASH = @"
                                                                                             88
                                                                                             88
                                                                                             88
                                                  ,adPPYba,  8b,dPPYba, ,adPPYYba, ,adPPYba, 88,dPPYba,
                                                 8b          88               P88  `''       88      88
                                                 8b          88        ,adPPPPP88  `''Y8ba,  88      88
                                                 ''8a,   ,aa 88        88,    ,88 aa     ]8I 88      88
                                                  `''Ybbd8'  88        `'8bbdP'Y8 `'YbbdP''  88      88     ";

        public static string HOLD_ON = @"
                                                   ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                               
                                                   )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_                                            
                                                                                                                                                    
                                                           ==== ++==  <>0 *##=== --_   (\ (\                                                  
                                                           +++--## # # == -= = == ---\\(TAT )  ^                                                
                                                                                                                                                
                                                   ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                                       
                                                   )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ";

        public static string[] TO_DROWN = { @"
                                            ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_                      _.====.._
                                                                                                               ,:._               
                                                    ==== ++==  <>0 *##=== --_   (\ (\                               \\            
                                                    +++--## # # == -= = == ---\\( TAT)  ^                            | _  _        
                                                                                                                     | _  _         
                                            ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                     / /_)/ | |    
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ", @" 
                                        ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_            _.====.._
                                                                                                    ,:._              ~-_
                                                    ==== ++==  <>0 *##=== --_   (\ (\                     \\           ~-_
                                                    +++--## # # == -= = == ---\\(oAO )  ^                   | _  _        `.
                                                                                                            | _  _        `.
                                            ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,             / /_)/ | |    ~-_  
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ", @" 
                                        ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ _.====.._
                                                                                        ,:._              ~-_
                                                    ==== ++==  <>0 *##=== --_   (\ (\         \\           ~-_
                                                    +++--## # # == -= = == ---\\(>A< )  ^       | _  _        `.
                                                                                                | _  _        `.
                                            ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-., / /_)/ | |    ~-_  
                                            )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ " };

        public static string[] DROWNING = { @"
                              ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                     ,                                              
                                                                      \(\  /)                                           
                                                                       (>\<)                                        
                                                                       (  V)                                        
                                                                        \  \         

", @"
                              ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                                    
                                                                      ,             *Blubbbublopp! Hnn?             
                                                                       \(\  /)                                          
                                                                        (>A<)                           
                                                                       ( `V)                                        
                                                                         \  \    
", @" 
                              ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                        
                                                                                                                                
                                                                                                                            
                                                                         \(\ /)/                                            
                                                                          (TO-)                                             
                                                                          (   ) *struggling                                 
                                                                           | |          " };

        public static string[] DROWNS = { @"
                               ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                                    
                                                                     ,                                                              
                                                                      \(\  /)                                   
                                                                       (>\<)                            
                                                                       (  V)                                
                                                                        \  \                
                                                                                                                            
                                                                                                                            
", @"
                               ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                            
                                                                      ,             *Blubbbublopp! Hnn?             
                                                                       \(\  /)                                              
                                                                        (>A<)                                               
                                                                       ( `V)                                                
                                                                         \  \                                                       
                                                                                                                                    
                                                                                                                            
", @"
                               ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                        
                                                                                                                    
                                                                                                                        
                                                                                                                        
                                                                         \(\ /)/                                                    
                                                                          (TO-)                                     
                                                                          (   ) *struggling                                                 
                                                                           | |                                                   
", @"
                               ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                        
                                                                                                                                
                                                                                                                                
                                                                                                                            
                                                                          (\ /)                                         
                                                                          (/^;)                                                     
                                                                         (   )\ *weakening                                              
                                                                         / /                                                
", @"                                                                                                                       
                               ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                                                                                        
                                                                                                                    
                                                                                                                    
                                                                                                                        
                                                                                                                        
                                                                                                                                
                                                                          (\ /)                                                     
                                                                          (- -)//                                           
                                                                           (   )=   " };

        public static string[] LET_GO = { @"
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                                  
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_                                    
                                                                                                                                          
                                                          ==== ++==  <>0 *##=== --_   (\ (\                                               
                                                          +++--## # # == -= = == ---\\( -.o)/                                         
                                                                                                                             
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                          
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_       ", @" 

                                                 ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_           
                                                                                                         
                                                          ==== ++==  <>0 *##=== --_     /) /)                 
                                                          +++--## # # == -= = == ---  ( /.o)/               
                                                                                                                
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,            
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ", @" 

                                                 ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`/) /) )`'-.            
                                                                                      ( /.<)/                 
                                                          ==== ++==  <>0 *##=== --_                       
                                                          +++--## # # == -= = == ---                          
                                                                                                    
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,    
                                                  _)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_     ", @"

                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)'-.,               
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_                      _.====.._
                                                                                                                     ,:._               
                                                          ==== ++==  <>0 *##=== --_                                      \\            
                                                          +++--## # # == -= = == ---                                       | _  _        
                                                                                                                           | _  _         
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                     / /_)/ | |    
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ", @" 

                                              ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                                          
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-._.====.._                                    
                                                                                             ,:._              ~-_                                
                                                          ==== ++==  <>0 *##=== --_               \\           ~-_                            
                                                          +++--## # # == -= = == ---                | _  _        `.                             
                                                                                                    | _  _        `.                      
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'/ /_)/ | |    ~-_                            
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ", @" 

                                              ,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,                          
                                                  )`'-.,_)`'-.,_)`'-.,_  .,_ _.====.._                                    
                                                                     ,:._              ~-_                                        
                                                          ==== ++==  <>0  \\            ~-_                                       
                                                          +++--## # # == -  | _  _        `.                                          
                                                                            | _  _        `.                                  
                                                  ,_)`'-.,_)`'-.,_)`'-.,_)` / /_)/ | |    ~-_                             
                                                  )`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_                                   " };

        public static string[] SWIM = { @"
                         /) /)    *splash
        )`'-.,_)`'-.   \( >`<)    )`'-.,_)`'-.,_)`'-.,_
                      <(  / )
                        >  >    ", @"
                                                 /) /)    *splash
                                )`'-.,_)`'-.   ( >`<) /    )`'-.,_)`'-.,_)`'-.,_
                                              <(\  )
                                                \  \   ", @"
                                                                             /) /)    *splash
                                                            )`'-.,_)`'-.   \( >`<)    )`'-.,_)`'-.,_)`'-.,_
                                                                          <(  / )
                                                                            >  >    " };

        public static string BEACH = @"
                                                                                                             /) /)    *splash
                                                                                            )`'-.,_)`'-.    ( >`<) /                ______
                                                                                                )`'-.,    <(\   )  )`'-.,    __.. , ,.-_/.:;
                                                                                                             \  \   . , ,  ,  _  -_## /...   ";

        public static string ARRIVE1 = @"


                                                                               (\   /)
                                                                               (     )
                                                                               /     \
                                                                             ` (  V  ) `
                                                                 _______________|___|____________
████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████ ";

        public static string ARRIVE2 = @"
                                                                            i   am  finally        

                                                             _______  ______   (\   /)   _______   _____
                                                             |  |  |  |  __|   (     )   | __  |  |  __|
                                                             |     |  |  __|   /     \   |    -|  |  __|
                                                             |__|__|  |____| ` (  V  ) ` |__|__|  |____|
                                                                 _______________|___|_______________
████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████ ";

        public static string BRIDGE = @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````





───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ 
███████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████ 



)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_";

        public static string SCARED = @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble
                                                                                               
                                                                                          
                                                            (\  /)                              
────────────────────────────────────────────────────────────( >^<)──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                                                                                            
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_ ";

        public static string[] WALK = { @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````



                
        (\  /)    Woahhh                                                                                                                                    
────────( `O`)/──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||   ) ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _\_ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
█████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████ 


                                                                                                                                                          
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                            *rumble
                                                                                                                                                *rumble

                
      *creak                    (\  /)   Need to be careful..                                                                                                                                                                                                  
────────────────────────────────( o^o)───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     || ( \ ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _/|| _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
█████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████ 


                                                                                                                                                          
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````_/ /````````````````````````````````````````````````
                            *rumble                                                                  __/ / 
                                                                                                   / __/     BOOOM                                  *rumble
                                                                                                  / /
                                                                                                 //                                 
                  *creak                                    (\  /)   EH?!                       //                                                            
────────────────────────────────────────────────────────────( 0A0)/──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (   ||     ||     ||              //          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _| \|| _ _ || _ _ || _           /        _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                      =   \     . ;  
                                                                                      \\          .  *CRASH
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                                                                
                                                                                                                    
                                                      *creak          The footings are gone...                                                                                                   
────────────────────────────────────────────────────────────(\  /)──!!───────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || ( Q.Q)     ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || ,.  )) _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                                                                            
                                                                                                                                                    
                                                                                                                                                          
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_" };

        public static string[] GO_BACK = { @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                             (\  /)    Wah.. that scared me..                                                               
──────────────────────────────────────────────────────────── ( Q^Q)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                                                                                      
                                                                                                                                                            
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble1
                                                                                               
                                                                                          
                                                            (\  /)                                                                                          
────────────────────────────────────────────────────────────( >^<)──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumble
                                                                                               
                                                                                          
                                                     (\  (\                                                                                                 
─────────────────────────────────────────────────────(Q^Q )───────────────────────────────  ────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     || (   ||     ||     ||     ||                         ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ \ || _ _ || _ _ || _ _ || _                   _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumble
                                                                                               
                                                                                          
                                         (\  (\                                                                                                            
─────────────────────────────────────────(Q^Q )─────────────────────                               ─────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||    (|| )   ||     ||     ||     ||     \                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _/|| _ _ || _ _ || _ _ || _ _ |                             _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
████████████████████████████████████████████████████████████████████████        \                    ███████████████████████████████████████████████████████ 
                                                                    █      █        
                                                                        █       █    
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumble
                                                                                               
                                                                                          
                                (\  (\  !!                                                                                                                 
────────────────────────────────( O^O)──────────                                                   ─────────────────────────────────────────────────────────
 ||     ||     ||     ||     || (  /||    (|| )    |                                                     ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ \ || _ _/|| _ _ || _ _                                             _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
█████████████████████████████████████████       ███ ████                        \                    ███████████████████████████████████████████████████████ 
                                                         █      █                                               
                                                                            █                           
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumble
                                                                                               
                                                                                          
                                                                                                                                                          
───────────────────────────                                                                       ─────────────────────────────────────────────────────────
 ||     ||     ||     |      |    (\ /)                                                                 ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ (/AO)/                                                            _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
███████████████████████████        (^,,^)                                                           ███████████████████████████████████████████████████████ 
                      █      █                                                                          
                            █                                                                                           
                                            █                                                                                                           
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumblE
                                                                                               
                                                                                          
                                                                                                                                                            
────                                                                                              ─────────────────────────────────────────────────────────
 ||                                                                                                     ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ |                                                                                           _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
█████████████                                                                                       ███████████████████████████████████████████████████████ 
                                                                
                                                                    
                                                                                                                                                      
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_
                        (\ /)           *splashh!!
        *splashh!!      (> <)/ *glub 
                         (   )=
" };

        public static string[] MUST = { @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                             (\  /)    Wah.. that scared me..                                                                                              
──────────────────────────────────────────────────────────── ( Q^Q)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                          
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                             (\  /)   I MUST!!                                                                              
──────────────────────────────────────────────────────────── ( O^O)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                                   (\  /)        RAAHHH!                                                                    
───────────────────────────────────────────────────────────────   ( OAO)/ ───────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||(V  `||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _/_>|| _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                   (\  /)   RAAHHH!         
                                                                                  ( OAO)/                                                                   
───────────────────────────────────────────────────────────────────────────────── (V`_^,^,  ─────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                        
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                      (\  /)  ???               
                                                                                     ( O^O)/                                                                              
───────────────────────────────────────────────────────────────────────────────────── (V`_^,^,   ────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                            
                                                                                                                                                                        
─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████     (\  /)         ███████████████████████████████████████████████████████ 
                                                                                      (/>A<)/                   
                                                                                   ,,(    )                     
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_ 


", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                            
                                                                                                           
─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                    ███████████████████████████████████████████████████████ 
                                                                                                            
                                                                                                            
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_
                                                                                          (\ /)           *splashh!!
                                                                           *splashh!!     (> <)/ *glub
                                                                                           (  )=      " };

        public static string[] SWING = { @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                     Wah.. that scared me..      
                                                             (\  /)     But I have to cross..                                                                                       
──────────────────────────────────────────────────────────── ( Q^Q)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                      
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                             (\  /)  Wait- Ropes..?                                                                         
──────────────────────────────────────────────────────────── ( `^`)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                            
                                                             (\  /)  Oh! They're sturdier than I thought!                                                                                   
──────────────────────────────────────────────────────────── ( .v.)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||  (\   )/  ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````/`````````````````````````````````````````````````````````````````````````````
                                                                            /                                                                          
                                                                          /                                                          *rumble
                                                                        /                        
                                                                      /                    
                                                             (\  /) *     *throw                                                                                                    
──────────────────────────────────────────────────────────── ( .v.)/────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||  (V   )   ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````/`````````````````````````````````````````````````````````````````````````````
                                                                             /                                                                          
                                                                           /                                                          *rumble
                                                                          /                        
                                                                         /                    
                                                                     (\* /)   *swing                                                                        
─────────────────────────────────────────────────────────────────── (//^`)──────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||  ( ^||^    ||                         ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````|`````````````````````````````````````````````````````````````````````````````
                                                                               \
                                                                                \                                                      *rumble
                                                                                 \                   
                                                                                  \            
                                                                                  (\  /)   *swing                                                                                                                              
─────────────────────────────────────────────────────────────────────────────────(`-*!)─────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ^^|                 ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````\`````````````````````````````````````````````````````````````````````````````
                                                                                \
                                                                                  \                                                      *rumble
                                                                                     \                   
                                                                                       \                
                                                                                         *(\  /)   *swing                                                                                                                         
──────────────────────────────────────────────────────────────────────────────────────────\(`-` )───────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||             ^^ )        ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````\`````````````````````````````````````````````````````````````````````````````
                                                                                  \
                                                                                     \                                                      *rumble
                                                                                         \                   
                                                                                             \            
                                                                                                *     (\  /)   *swing                                                                                                              
──────────────────────────────────────────────────────────────────────────────────────────────────\  (`-` )─────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                     ^ )\\      ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ \ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````\`````````````````````````````````````````````````````````````````````````````
                                                                                \          
                                                                                  \                                                      *rumble
                                                                                    \                   
                                                                                      \            
                                                                                        \              (\  /)      Wahhh! I made it!                                                                 
───────────────────────────────────────────────────────────────────────────────────────────────────────( ^O^)───────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                      ( \ /)    ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ / || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_", @"
``````````````````````````````````````````````````````````````````````````````\`````````````````````````````````````````````````````````````````````````````
                                                                               \         
                                                                                \                                                      *rumble
                                                                                 \                   
                                                                                  \            
                                                                                   \                            (\  /)      Must hurry!                     
────────────────────────────────────────────────────────────────────────────────────────────────────────────────( ovo)──────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||     ||                         ||    ( V`V`  ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _                   _ _ || _ _ ||\_ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                       
                                                                                                                                                        
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_" };

        public static string[] LIMIT = { @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                     *rumble                                                                 
                                                                                                                                   *rumble
                                                                                               
                                                                                          
                                                             (\  /)    Wah.. that scared me..                                                                
──────────────────────────────────────────────────────────── ( Q^Q)─────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                                       
                                                                                        
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble
                                                                                               
                                                                                                            
                                                            (\  /)   WAAAHHHHHHHH I CANT DECIDE                                                             
────────────────────────────────────────────────────────────( >^<)──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||     ||                          ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ || _ _ || _                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
██████████████████████████████████████████████████████████████████████████████████                   ███████████████████████████████████████████████████████ 
                                                                             █         
                                                                                 █                                                                           
                                                                        █                                                                                
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble
                                                                                               
                                                                                                                                
                                                            (\  /)   WAAAHHHHHHHH I CANT DECIDE                                                             
────────────────────────────────────────────────────────────( >^<)──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)    ||                                 ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | | _ _ ||                             _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
███████████████████████████████████████████████████████████████████                                  ███████████████████████████████████████████████████████ 
                                                               █        █                                                                                   
                                                                   █         █                                                                              
                                                                        █                                                                                 
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble
                                                                                               
                                                                                                                        
                                                            (\  /)  EHH?                                                                                    
────────────────────────────────────────────────────────────( .A.)──────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||     ||     ||     || (\  /)                                       ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||   | |                                    _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
█████████████████████████████████████████████████████████                                            ███████████████████████████████████████████████████████ 
                                                       █                            
                                                    █        █                                                                                              
                                                                     █                                                                                  
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                               *rumble                                                                 
                     *rumble
                                                                                               
                                                                                          
                                                                                                                                                                                       
────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
 ||     ||     ||     ||     ||     ||                                                                   ||     ||     ||     ||     ||     ||     ||     ||
 || _ _ || _ _ || _ _ || _ _ || _ _ || _                     (\ /)                                   _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ ||
███████████████████████████████████████                      (/AO)/                                  ███████████████████████████████████████████████████████ 
                                                              (^,,^)                                                                                        
                                                                                                                                                            
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_



", @"
````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````
                                                                                                                                            *rumble                                                                 
                                                        *rumble
                                                                                               
                                                                                          
                                                                                                                                                           
────                                                                                               ───────────────────────────────────────────────────────── 
 ||                                                                                                      ||     ||     ||     ||     ||     ||     ||     || 
 || _ _ |                                                                                            _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || _ _ || 
█████████████                                                                                        ███████████████████████████████████████████████████████  
                                                                                                                                                            
                                                                                                                                                            
                                                                                                                                                            
)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,)`'-.,_)`'-.,_)`'-.,_)`'-.,_
                                                              (\ /)           *splashh!!
                                              *splashh!!      (> <)/ *glub 
                                                               (   )=
" };

        public void PlayAnimation()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }

    internal class AgeDefiner : playerDetails
    {
        ColoredText color = new ColoredText();
        HistoryLog historyLog = new HistoryLog();

        public void DefineAge()
        {
            while (true)
            {
                Console.Write("\n\t\tAge: ");
                string? age = Console.ReadLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> entered their AGE");

                if (string.IsNullOrWhiteSpace(age) || !int.TryParse(age, out int ageValue))
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\nINVALID INPUT.\n\n\n", ConsoleColor.Red);  Thread.Sleep(1000);
                    Console.Clear();
                    for (int i = 0; i < 3; i++) Console.WriteLine(); Thread.Sleep(1500);
                    continue;
                }

                if (ageValue <= 9 || ageValue >= 51)
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\nINVALID AGE ENTERED.\n\n\n", ConsoleColor.Red); Thread.Sleep(1000);
                    Console.WriteLine(Animation.invalidage); Thread.Sleep(1500);
                    Console.Clear();
                    for (int i = 0; i < 3; i++) Console.WriteLine(); Thread.Sleep(1500);
                    continue;
                }

                historyLog.QuickLog("SYSTEM", "AGE entered is valid");
                playerDetails.age = ageValue;
                for (int i = 0; i < 5; i++) Console.WriteLine();
                break;
            }
        }
    }

    public class ColoredText
    {
        private int GetLeftPadding(string text)
        {
            int consoleWidth = Console.WindowWidth;
            return Math.Max((consoleWidth - text.Length) / 2, 0);
        }

        public void TEXT(string text, ConsoleColor color = ConsoleColor.White, bool newLine = true)
        {
            Console.ForegroundColor = color;

            string[] lines = text.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine();
                    continue;
                }

                int leftPadding = Math.Max((Console.WindowWidth - line.Length) / 2, 0);
                if (newLine) Console.Write(new string(' ', leftPadding) + line);
                else Console.Write(new string(' ', leftPadding) + line);
            }
            Console.ResetColor();
        }


        public void BOTH(string text, ConsoleColor fgcolor, ConsoleColor bgcolor, bool newLine = true)
        {
            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;
            if (newLine) Console.Write(text);
            else Console.Write(text);
            Console.ResetColor();
        }
    }

    internal class Intro : playerDetails, IAnimation
    {
        public static string prompt = @" 

                                                        (Use the 'Arrow Keys'.Press 'Enter' to select.)




.  .  _ .       
|\/| [_ |\ | |  | *
|  | [_ | \| |__| *
";

        public void PlayAnimation()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public void START()
        {
            HistoryLog historyLog = new HistoryLog();

            bool running = true;
            while (running)
            {
                Console.WriteLine(Animation.title);

                ColoredText color = new ColoredText();

                string[] choices = { "New Game", "Continue", "About", "History Log", "Exit" };
                Menu mainMenu = new Menu(prompt, choices);
                int selectedIndex = mainMenu.Run();

                switch (selectedIndex)
                {
                    case 0:
                        NewGame newgame = new NewGame();
                        newgame.START();
                        break;

                    case 1:
                        Continue.AskForUsername();
                        Continue cont = new Continue();

                        if (!cont.HasSavedProgress())
                        {
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("\n\n\n\nNo saved progress found. Please start a New Game first.", ConsoleColor.Red);
                            historyLog.QuickLog("SYSTEM", "Attempted to continue without saved progress.");
                            Thread.Sleep(2000);
                            Console.Clear();
                            break; 
                        }
                        cont.LoadAndResume();
                        break;

                    case 2:
                        About about = new About();
                        about.GameInfo();
                        break;

                    case 3:
                        historyLog.ShowLog();
                        break;

                    case 4:
                        Exit exitAnim = new Exit();
                        exitAnim.PlayAnimation();
                        return;
                    default:
                        Console.Clear();
                        color.TEXT("\n\nINVALID OPTION. TRY AGAIN.\n\n\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        return;
                }

                Console.Write("\n\nPress ENTER to return to the menu...");
                historyLog.QuickLog("SYSTEM", "Returning to the Menu");
                Console.ReadLine();
                Console.Clear();
            }
        }
        
        internal class NewGame : playerDetails
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void End1() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                             "\n\n\n\n\n\n\t\tJoe Dichiara >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t\t 'Without The Will To Start, There Can Be No Journey.", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t It's Tough Sometimes To Take That First Step, But It Becomes Easier Once You're On Your Way.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 7; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'NOT YET' Ending Route Discovered");
                return;
            }

            public void START() //newgame
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> opened 'New Game' Menu");
                while (true)
                {
                    Console.Clear();

                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                    Console.Write("\n\n\t\tEnter a nickname of your desire: ");
                    userName = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\nINVALID NAME ENTERED.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        continue;
                    }
                   
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> entered their NAME");
                    Continue.SetCurrentUser(userName);
                    break;
                }

                AgeDefiner ageChecker = new AgeDefiner();
                ageChecker.DefineAge();
                for (int i = 0; i < 3; i++) Console.WriteLine();
                for (int i = 0; i < 5; i++)
                {
                    color.TEXT("\n .\n\n", ConsoleColor.Green);
                    Thread.Sleep(1000);
                }

                Console.Clear();

                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                for (int i = 0; i < 8; i++) Console.WriteLine();

                int ageValue = playerDetails.age;

                if (ageValue >= 10 && ageValue <= 20)
                {
                    Typing.AnimateType("\t\t\tOnce, there was a 'little one'...", ConsoleColor.Green, "left");
                }

                else if (ageValue >= 21 && ageValue <= 40)
                {
                    Typing.AnimateType("\t\t\tOnce, there was a 'dreamer'...", ConsoleColor.Green, "left");
                }

                if (ageValue >= 41 && ageValue <= 50)
                {
                    Typing.AnimateType("\t\t\tOnce, there was an 'old soul'...", ConsoleColor.Green, "left");
                }


                Typing.AnimateType("\n\n\n\t\t\t\tAll alone in a desolate and unfamiliar place, although they didn't realize it yet..." +
                           "\n\n\n\t\t\t\t\t\t\t\tA guardian beyond the screen even gave them a lovely name." +
                          $"\n\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t< {userName} > is what they are called.", ConsoleColor.Green, "left");

                int currentY = Console.CursorTop + 8;
                Typing.AnimateFrames(Animation.wakesup, repeat: 1, delay: 500, yOffset: currentY); Thread.Sleep(1000);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Typing.AnimateType("The child  finally wakes from their slumber.", ConsoleColor.Green, "center"); Thread.Sleep(1000);

                for (int i = 0; i < 8; i++) Console.WriteLine();
                Typing.AnimateType("Huhh?? Where am I? Why is it so dark??", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);

                for (int i = 0; i < 8; i++) Console.WriteLine();
                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\u001b[5m I See You're Finally Awake, Esteemed Player! We Have Been Waiting For You! \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2500);

                for (int i = 0; i < 8; i++) Console.WriteLine();
                Typing.AnimateType("H-huh? Who-! What are you??", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.confused
                    [0].Split('\n').Length);
                Typing.AnimateFrames(Animation.confused, repeat: 1, delay: 800, yOffset: safeY); Thread.Sleep(1000);
                for (int i = 0; i < 13; i++) Console.WriteLine();

                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\u001b[5mI Am The [ $Y$73M ]! And You Are Here For A Special Reason. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m      Of Course, You Have To Find That For Yourself. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m     But Fret Not, I Will Be With You In This Journey. \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t     \u001b[5m So, Are You Ready <{userName}>? (Y/N): \u001b[0m");
                    string? ready = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                
                    if (ready == "n" || ready == "N")
                    {
                        saveSystem.SaveCheckpoint("End1", "");
                        Continue.ContinueOrExit();

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\tJoe Dichiara >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'Without The Will To Start, There Can Be No Journey.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t It's Tough Sometimes To Take That First Step, But It Becomes Easier Once You're On Your Way.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 7; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'NOT YET' Ending Route Discovered");
                        return;
                    }

                    else if (ready == "y" || ready == "Y")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> officially opens the game");

                        Console.Clear();
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                        Console.WriteLine(Animation.welcome);
                        for (int i = 0; i < 3; i++) Console.WriteLine();
                        for (int i = 0; i < 3; i++)
                        {
                            color.TEXT("\n .\n\n", ConsoleColor.Green);
                            Thread.Sleep(500);
                        }
                        Thread.Sleep(1500);

                        Trial1 trial1 = new Trial1();
                        trial1.PlayAnimation();

                        Trial2 trial2 = new Trial2(this);
                        trial2.PlayAnimation();

                        break;
                    }


                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }
    }

    internal class Trial1 : NewGame, IAnimation, IChecks
    {
        ColoredText color = new ColoredText();
        HistoryLog historyLog = new HistoryLog();
        Continue saveSystem = new Continue();

        public void SaveCheckpoint(string choice)
        {
            saveSystem.SaveCheckpoint("Trial1", choice);
        }

        public void ContinueAnimation(string choice) //continue
        {
            Console.Clear();
            Console.WriteLine($"\n\tContinuing to First Trial from Choice {choice}..."); Thread.Sleep(2500);
            Console.Clear();
            int safeY;

            if (choice == "1")
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to participate with the warm-up");
                Continue.ContinueOrExit();
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.STRAIGHT[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.STRAIGHT, repeat: 1, delay: 1000, yOffset: safeY);
            }
            else if (choice == "2")
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to jump UP!");
                Continue.ContinueOrExit();
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.JUMP[0].Split('\n').Length);
                Typing.AnimateJump(Animation.JUMP, startY: safeY, jumpHeight: 2, delay: 250);
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.UP[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.UP, repeat: 1, delay: 1000, yOffset: safeY);
            }
            else if (choice == "3")
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to jump DOWN!");
                Continue.ContinueOrExit();
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.JUMP[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.JUMP, repeat: 1, delay: 1000, yOffset: safeY);
                safeY = Math.Min(Console.BufferHeight - 5, Console.CursorTop + Animation.DOWN[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.DOWN, repeat: 1, delay: 1000, yOffset: safeY);
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Console.Clear();
            }

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

            TeaTime tea = new TeaTime();
            tea.StartTeaTime();
        }

        public void PlayAnimation() //newgame
        {
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Typing.AnimateType("So... I guess I'll start walking then?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);
            int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.start[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.start, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);

            for (int i = 0; i < 15; i++) Console.WriteLine();
            Typing.AnimateType("Phew... I wonder how much longer do I have to walk...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);
            for (int i = 0; i < 8; i++) Console.WriteLine();
            safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.startwalk[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.startwalk, repeat: 5, delay: 500, yOffset: safeY); Thread.Sleep(1000);

            for (int i = 0; i < 15; i++) Console.WriteLine();
            while (true)
            {
                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write("\n\n\t\t\t\t\t    \u001b[5m Getting Impatient, I see. How About We Have A Little Game Instead? \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 1) Nah, Go Straight Ahead \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 2) Let's Go Up! \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 3) Let's Go Down! \u001b[0m");
                Console.Write($"\n\n\n\t\t\t\t\t\t\t \u001b[5m Choice: \u001b[0m");
                string? choice = Console.ReadLine();
                Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);
                historyLog.QuickLog("SYSTEM", "An option is given");

                Console.Clear();
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                if (choice == "1")
                {
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to participate with the warm-up");
                    SaveCheckpoint(choice);
                    Continue.ContinueOrExit();
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.STRAIGHT[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.STRAIGHT, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                    break;
                }

                else if (choice == "2")
                {
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to jump UP!");
                    SaveCheckpoint(choice);
                    Continue.ContinueOrExit();
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.UP[0].Split('\n').Length);
                    Typing.AnimateJump(Animation.JUMP, startY: safeY, jumpHeight: 2, delay: 250);
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.UP[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.UP, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                    break;
                }

                else if (choice == "3")
                {
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to jump DOWN!");
                    SaveCheckpoint(choice);
                    Continue.ContinueOrExit();
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.JUMP[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.JUMP, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(200);
                    safeY = Math.Min(Console.BufferHeight - 5, Console.CursorTop + Animation.DOWN[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.DOWN, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                    break;
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Console.Clear();
                }
            }

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

            TeaTime tea = new TeaTime();
            tea.StartTeaTime();
        }

        public void ContinueTea(string tea) //continue
        {
            Console.Clear();
            Console.WriteLine($"\n\tContinuing Tea Time From Choice {tea}..."); Thread.Sleep(2500);
            int safeY;
            Console.Clear();
            Continue.ContinueOrExit();

            if (tea == "1")
            {
                for (int i = 0; i < 8; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to participate with the Tea Time");
                saveSystem.SaveCheckpoint("TeaTime", tea);
                Continue.ContinueOrExit();

                for (int i = 0; i < 10; i++) Console.WriteLine();
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.IGNORE[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.IGNORE, repeat: 1, delay: 700, yOffset: safeY); Thread.Sleep(1000);
                for (int i = 0; i < 10; i++) Console.WriteLine();
                for (int i = 0; i < 3; i++)
                {
                    color.TEXT("\n .\n\n", ConsoleColor.Green);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                for (int i = 0; i < 4; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
            }

            else if (tea == "2")
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> hesitantly accepted the Tea offer");
                saveSystem.SaveCheckpoint("TeaTime", tea);
                Continue.ContinueOrExit();
                for (int i = 0; i < 10; i++) Console.WriteLine();
                int startY = Console.CursorTop;
                safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.SURE, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                historyLog.QuickLog("SYSTEM", "A teacup suddenly appeared, flying towards the <<PLAYER>>");
                for (int i = 0; i < 13; i++) Console.WriteLine();
                Typing.AnimateType("The child kept walking, uncertain of where they would end up.\n", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
            }

            else if (tea == "3")
            {
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> happily  accepted the Tea offer");
                saveSystem.SaveCheckpoint("TeaTime", tea);
                Continue.ContinueOrExit();
                for (int i = 0; i < 10; i++) Console.WriteLine();

                Console.WriteLine(Animation.UNSURE); Thread.Sleep(1000);
                for (int i = 0; i < 15; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "A teacup suddenly appeared, flying towards the <<PLAYER>>");

                int startY = Console.CursorTop;
                safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.SURE, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 18; i++) Console.WriteLine();
                Typing.AnimateType("The child kept walking, uncertain of where they would end up.\n", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> encounters an obstacle");
            for (int i = 0; i < 5; i++) Console.WriteLine();
            safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.STOP[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.STOP, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
            for (int i = 0; i < 15; i++) Console.WriteLine();
            Typing.AnimateType("The child stopped walking upon seeing a difficult obstacle.\n\n\n", ConsoleColor.Green, "center"); Thread.Sleep(3000);

            Trial1_door trial_1 = new Trial1_door(new playerDetails());
            trial_1.Door();
        }

        public class TeaTime() //newgame
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void StartTeaTime()
            {
                Trial1 trial = new Trial1();
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t\t\t      \u001b[5m Wow! Good Job! \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.TIRED[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.TIRED, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                for (int i = 0; i < 15; i++) Console.WriteLine();

                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t     \u001b[5m Oh? I See You're Tired Already. How About We Have A Short Break? \u001b[0m");
                    Console.Write("\n\t\t\t\t\t\t\t\t    \u001b[5m Some Tea Perhaps? \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 1) Hmm.. No Thanks, I think  I'll Manage. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 2) That Would Be Very Helpful, Thanks! \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 3) Hhmmm... I Guess I Should Do That...? \u001b[0m");
                    Console.Write($"\n\n\n\t\t\t\t\t\t\t \u001b[5m Choice: \u001b[0m");
                    string? tea = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    historyLog.QuickLog("SYSTEM", "An option is given");
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    if (tea == "1")
                    {
                        for (int i = 0; i < 8; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to participate with the Tea Time");
                        saveSystem.SaveCheckpoint("TeaTime", tea);
                        Continue.ContinueOrExit();

                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.IGNORE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.IGNORE, repeat: 1, delay: 700, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        for (int i = 0; i < 3; i++)
                        {
                            color.TEXT("\n .\n\n", ConsoleColor.Green);
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(1000);
                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
                        break;
                    }

                    else if (tea == "2")
                    {
                        for (int i = 0; i < 15; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> hesitantly accepted the Tea offer");
                        saveSystem.SaveCheckpoint("TeaTime", tea);
                        Continue.ContinueOrExit();

                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        int startY = Console.CursorTop;
                        safeY = Math.Min(startY, Console.BufferHeight - 8);
                        Typing.AnimateFrames(Animation.SURE, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);
                        historyLog.QuickLog("SYSTEM", "A teacup suddenly appeared, flying towards the <<PLAYER>>");

                        for (int i = 0; i < 18; i++) Console.WriteLine();
                        Typing.AnimateType("The child kept walking, uncertain of where they would end up.", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
                        break;
                    }

                    else if (tea == "3")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> happily  accepted the Tea offer");
                        saveSystem.SaveCheckpoint("TeaTime", tea);
                        Continue.ContinueOrExit();
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        Console.WriteLine(Animation.UNSURE); Thread.Sleep(1000);
                        for (int i = 0; i < 15; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "A teacup suddenly appeared, flying towards the <<PLAYER>>");

                        int startY = Console.CursorTop;
                        safeY = Math.Min(startY, Console.BufferHeight - 8);
                        Typing.AnimateFrames(Animation.SURE, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                        for (int i = 0; i < 15; i++) Console.WriteLine();
                        Typing.AnimateType("The child kept walking, uncertain of where they would end up.", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
                        break;
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> encounters an obstacle");
                for (int i = 0; i < 5; i++) Console.WriteLine();
                safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.STOP[0].Split('\n').Length);
                Typing.AnimateFrames(Animation.STOP, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                for (int i = 0; i < 15; i++) Console.WriteLine();
                Typing.AnimateType("The child stopped walking upon seeing a difficult obstacle.\n\n\n", ConsoleColor.Green, "center"); Thread.Sleep(3000);

                Trial1_door trial_1 = new Trial1_door(new playerDetails());
                trial_1.Door();
            }
        }

        public void ContinueAgain(string door)  //continue
        {
            Console.Clear();
            Console.WriteLine($"\n\tContinuing Door Trial From Choice {door}..."); Thread.Sleep(2500);
            int safeY;
            Console.Clear();
            Continue.ContinueOrExit();

            if (door == "1" || door == "2" || door == "3" || door == "4" || door == "5" || door == "6")
            {
                for (int i = 0; i < 5; i++) Console.WriteLine();
                Typing.AnimateType("\n\n\n\t\t\t\t\t\t\t\t\t\tI think I'll go for...", ConsoleColor.DarkYellow, "left"); Thread.Sleep(1000);

                if (door == "1")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ FOOD ] door");
                    Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ FOOD ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                else if (door == "2")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SLEEP ] door");
                    Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SLEEP ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                else if (door == "3")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ GAMING ] door");
                    Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ GAMING ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                else if (door == "4")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SUBSTANCE ADDICTION ] door");
                    Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SUBSTANCE ADDICTION ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                else if (door == "5")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ ENTERTAINMENT ] door");
                    Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ ENTERTAINMENT ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                else if (door == "6")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SOCIAL MEDIA PLATFORMS ] door");
                    Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SOCIAL MEDIA PLATFORMS ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                }

                for (int i = 0; i < 5; i++) Console.WriteLine();
                int startY = Console.CursorTop;
                int frameHeight = Animation.OPENDOOR[0].Split('\n').Length;
                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                Typing.AnimateFrames(Animation.OPENDOOR, repeat: 1, delay: 700, yOffset: safeY); Thread.Sleep(1500);
                for (int i = 0; i < 15; i++) Console.WriteLine();

                Console.WriteLine(Animation.HAPPY); Thread.Sleep(3000);
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Console.Clear();
            }

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            for (int i = 0; i < 40; i++) Console.WriteLine(new string(' ', Console.WindowWidth));
            Thread.Sleep(300);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();

            Console.SetCursorPosition(0, 10);
            color.TEXT("W-WAHH!! Why is it so bright??", ConsoleColor.DarkYellow);

            Thread.Sleep(3000);
            Console.ResetColor();

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.DISTRICT);
            for (int i = 0; i < 7; i++) Console.WriteLine(); Thread.Sleep(3000);

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWELCOME TO THE DISTRICT OF DESIRES!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(3000);
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> unknowingly entered the 'District of Pleasures', a place of temptation");

            if (door == "1")
            {
                Door1 door1 = new Door1();
                door1.Door1Scene();
            }

            else if (door == "2")
            {
                Door2 door2 = new Door2();
                door2.Door2Scene();
            }

            else if (door == "3")
            {
                Door3 door3 = new Door3();
                door3.Door3Scene();
            }

            else if (door == "4")
            {
                Door4 door4 = new Door4();
                door4.Door4Scene();
            }

            else if (door == "5")
            {
                Door5 door5 = new Door5();
                door5.Door5Scene();
            }

            else if (door == "6")
            {
                Door6 door6 = new Door6();
                door6.Door6Scene();
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Console.Clear();
            }

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            for (int i = 0; i < 40; i++) Console.WriteLine(new string(' ', Console.WindowWidth));
            Thread.Sleep(300);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Clear();

            Console.SetCursorPosition(0, 10);
            color.TEXT("W-WAHH!! Why is it so bright??", ConsoleColor.DarkYellow);

            Thread.Sleep(3000);
            Console.ResetColor();

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.DISTRICT);
            for (int i = 0; i < 7; i++) Console.WriteLine(); Thread.Sleep(3000);

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWELCOME TO THE DISTRICT OF DESIRES!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(3000);
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> unknowingly entered the 'District of Pleasures', a place of temptation");

            if (door == "1")
            {
                Door1 door1 = new Door1();
                door1.Door1Scene();
            }

            else if (door == "2")
            {
                Door2 door2 = new Door2();
                door2.Door2Scene();
            }

            else if (door == "3")
            {
                Door3 door3 = new Door3();
                door3.Door3Scene();
            }

            else if (door == "4")
            {
                Door4 door4 = new Door4();
                door4.Door4Scene();
            }

            else if (door == "5")
            {
                Door5 door5 = new Door5();
                door5.Door5Scene();
            }

            else if (door == "6")
            {
                Door6 door6 = new Door6();
                door6.Door6Scene();
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Console.Clear();
            }
        }

        public class Trial1_door
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();
            private playerDetails _player;

            public Trial1_door(playerDetails player)
            {
                _player = player;
            }

            public void Again() //newgame
            {
                Console.WriteLine(Animation.WALL);
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> starts the 1st Trial");

                Console.Clear();
                for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(500);
                Typing.AnimateType("Where should I go?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(3000);
                for (int i = 0; i < 10; i++) Console.WriteLine();

                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Options: \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 1) Left \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 2) Right \u001b[0m");
                    Console.Write($"\n\n\n\t\t\t\t\t\t\t \u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    if (choice == "1")
                    {
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.WriteLine(Animation.DOOR); Thread.Sleep(500);
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.DOOR_P); Thread.Sleep(3000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go to the mysterious doors");

                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        string? door = null;
                        while (true)
                        {
                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                            door = Console.ReadLine();
                            Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2500);

                            if (door == "1" || door == "2" || door == "3" || door == "4" || door == "5" || door == "6")
                            {
                                break;
                            }

                            else
                            {
                                Console.Clear();
                                historyLog.QuickLog("SYSTEM", "ERROR");
                                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                                Thread.Sleep(1000);
                                Console.Clear();
                            }
                        }

                        saveSystem.SaveCheckpoint("DoorChoice", door);
                        Continue.ContinueOrExit();
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Typing.AnimateType("\n\n\n\t\t\t\t\t\t\t\t\t\tI think I'll go for...", ConsoleColor.DarkYellow, "left"); Thread.Sleep(1000);

                        if (door == "1")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ FOOD ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ FOOD ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }

                        else if (door == "2")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SLEEP ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SLEEP ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }

                        else if (door == "3")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ GAMING ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ GAMING ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }

                        else if (door == "4")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SUBSTANCE ADDICTION ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SUBSTANCE ADDICTION ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }

                        else if (door == "5")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ ENTERTAINMENT ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ ENTERTAINMENT ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }

                        else if (door == "6")
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to enter the [ SOCIAL MEDIA PLATFORMS ] door");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\t\t  the [ SOCIAL MEDIA PLATFORMS ] one!", ConsoleColor.Yellow, "left"); Thread.Sleep(1000);
                        }
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        int startY = Console.CursorTop;
                        int frameHeight = Animation.OPENDOOR[0].Split('\n').Length;
                        int safeY = Typing.GetSafeYOffset(startY, frameHeight);
                        Typing.AnimateFrames(Animation.OPENDOOR, repeat: 1, delay: 700, yOffset: safeY); Thread.Sleep(1500);

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;

                        for (int i = 0; i < 40; i++) Console.WriteLine(new string(' ', Console.WindowWidth));
                        Thread.Sleep(300);

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Clear();

                        Console.SetCursorPosition(0, 10);
                        color.TEXT("W-WAHH!! Why is it so bright??", ConsoleColor.DarkYellow);

                        Thread.Sleep(3000);
                        Console.ResetColor();

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.DISTRICT);
                        for (int i = 0; i < 7; i++) Console.WriteLine(); Thread.Sleep(3000);

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWELCOME TO THE DISTRICT OF DESIRES!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(3000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> unknowingly entered the 'District of Pleasures', a place of temptation");

                        if (door == "1")
                        {
                            Door1 door1 = new Door1();
                            door1.Door1Scene();
                        }

                        else if (door == "2")
                        {
                            Door2 door2 = new Door2();
                            door2.Door2Scene();
                        }

                        else if (door == "3")
                        {
                            Door3 door3 = new Door3();
                            door3.Door3Scene();
                        }

                        else if (door == "4")
                        {
                            Door4 door4 = new Door4();
                            door4.Door4Scene();
                        }

                        else if (door == "5")
                        {
                            Door5 door5 = new Door5();
                            door5.Door5Scene();
                        }

                        else if (door == "6")
                        {
                            Door6 door6 = new Door6();
                            door6.Door6Scene();
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Console.Clear();
                        }
                    }

                    else if (choice == "2")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go visit the dead end to see if they can cross over");

                        Trial1_wall wall = new Trial1_wall(_player);
                        wall.Wall();
                        break;
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }

            public void Door()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                for (int i = 0; i < 8; i++) Console.WriteLine();

                Console.WriteLine(Animation.OBSTACLE); Thread.Sleep(1000);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Typing.AnimateType("Why are there 2 Paths?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 5; i++) Console.WriteLine();
                Typing.AnimateType("On the left side, the child could see a wider path with doors ahead...", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 10; i++) Console.WriteLine();

                Console.WriteLine(Animation.DOOR); Thread.Sleep(1000);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Typing.AnimateType("While it's just a dead-end on the right side...", ConsoleColor.Green, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 2; i++) Console.WriteLine();
                Typing.AnimateType("Hmm...?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);
                Typing.AnimateType("\n\n\nI think I see a path after the wall?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(1000);
                for (int i = 0; i < 10; i++) Console.WriteLine();

                Trial1_door trial = new Trial1_door(new playerDetails());
                trial.Again();
            }
        }

        public void ContinueWall(string choice) //continue
        {
            Console.Clear();
            Console.WriteLine($"\n\tContinuing Wall Trial From Choice {choice}..."); Thread.Sleep(2500);
            int safeY;
            Console.Clear();

            if (choice == "y" || choice == "Y")
            {
                for (int i = 0; i < 25; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to climb the wall");
                saveSystem.SaveCheckpoint("WallChoice", choice);
                Continue.ContinueOrExit();

                int startY = Console.CursorTop;
                int frameHeight = Animation.PRE_CLIMB[0].Split('\n').Length;
                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                Typing.AnimateFrames(Animation.PRE_CLIMB, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 27; i++) Console.WriteLine();
                Typing.AnimateType("Ouch that looks painful...", ConsoleColor.Green, "center"); Thread.Sleep(400);

                for (int i = 0; i < 8; i++) Console.WriteLine();
                bool finished = false;

                while (!finished)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t   \u001b[5m Will you still push through and climb the wall[1] or will you walk away instead[2]?\u001b[0m"); Thread.Sleep(400);
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choose = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    if (choose == "1")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> still choose to climb the wall");
                        Console.Clear();
                        for (int i = 0; i < 30; i++) Console.WriteLine();
                        startY = Console.CursorTop;
                        frameHeight = Animation.CLIMB[0].Split('\n').Length;
                        safeY = Typing.GetSafeYOffset(startY, frameHeight);
                        Typing.AnimateFrames(Animation.CLIMB, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);

                        for (int i = 0; i < 25; i++) Console.WriteLine();
                        Typing.AnimateType("No! I was almost there!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        while (true)
                        {
                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            color.TEXT("\n\n\u001b[5m You almost got it but still fell... Much painful this time too...\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                            color.TEXT("\n\n\u001b[5m Maybe if you give it another try, I'm sure things will work out!\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                            color.TEXT("\n\n\u001b[5m Don't give up just yet! So- will you give it your all this time and climb that wall?\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                            color.TEXT("\n\n\u001b[5m Choice(Y/N): \u001b[0m");
                            string? choose1 = Console.ReadLine();
                            Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "An option is given");

                            if (choose1 == "y" || choose1 == "Y")
                            {
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> still choose to climb the wall");
                                for (int i = 0; i < 30; i++) Console.WriteLine();
                                startY = Console.CursorTop;
                                frameHeight = Animation.SUCCESS[0].Split('\n').Length;
                                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                Typing.AnimateFrames(Animation.SUCCESS, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully crossed over the wall!");

                                for (int i = 0; i < 30; i++) Console.WriteLine();
                                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                Console.Write($"\n\n\t\t\t\t\t\t\t\t     \u001b[5mWow, great job! \u001b[0m");
                                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                Thread.Sleep(2000);
                                finished = true; break;
                            }

                            else if (choose1 == "n" || choose1 == "N")
                            {
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to give up climbing the wall");
                                for (int i = 0; i < 5; i++) Console.WriteLine();
                                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m DONT'T GIVE UP! \u001b[0m");
                                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                Thread.Sleep(2000);
                                for (int i = 0; i < 15; i++) Console.WriteLine();
                                historyLog.QuickLog("SYSTEM", "I wont allow them, of course. They're so near.");

                                startY = Console.CursorTop;
                                frameHeight = Animation.AGAIN[0].Split('\n').Length;
                                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                Typing.AnimateFrames(Animation.AGAIN, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                                for (int i = 0; i < 30; i++) Console.WriteLine();

                                startY = Console.CursorTop;
                                frameHeight = Animation.CLIMB[0].Split('\n').Length;
                                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                Typing.AnimateFrames(Animation.SUCCESS, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully crossed over the wall!");
                                for (int i = 0; i < 30; i++) Console.WriteLine();
                                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5mWow, great job! \u001b[0m");
                                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                Thread.Sleep(2000);
                                finished = true; break;
                            }

                            else
                            {
                                Console.Clear();
                                historyLog.QuickLog("SYSTEM", "ERROR");
                                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                                Thread.Sleep(1000);
                                Console.Clear();
                            }
                        }
                        break;
                    }

                    else if (choose == "2")
                    {
                        for (int i = 0; i < 35; i++) Console.WriteLine();
                        startY = Console.CursorTop;
                        frameHeight = Animation.CLIMB[0].Split('\n').Length;
                        safeY = Typing.GetSafeYOffset(startY, frameHeight);
                        Typing.AnimateFrames(Animation.FAIL, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to stop climbing the wall");

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> walks away from the wall");
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OBSTACLE); for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(400);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000);

                        Trial1_door trial = new Trial1_door(new playerDetails());
                        trial.Again();
                        finished = true; break;
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }

            else if (choice == "n" || choice == "N")
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to climb the wall");
                saveSystem.SaveCheckpoint("WallChoice", choice);
                Continue.ContinueOrExit();

                for (int i = 0; i < 10; i++) Console.WriteLine();
                int startY = Console.CursorTop;
                int frameHeight = Animation.FAIL[0].Split('\n').Length;
                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                Typing.AnimateFrames(Animation.FAIL, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> walks away from the wall");
                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.OBSTACLE); for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(2000);
                for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(400);

                Trial1_door trial = new Trial1_door(new playerDetails());
                trial.Again();
            }

            else
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                Thread.Sleep(1000);
                Console.Clear();
            }

            Trial2 trial2 = new Trial2(this);
            trial2.PlayAnimation();
        }

        public class Trial1_wall
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            private playerDetails _player;

            public Trial1_wall(playerDetails player)
            {
                _player = new playerDetails(); //_player = player;
            }

            public void Wall() //newgame
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                Console.WriteLine(Animation.WALL); Thread.Sleep(3000);
                for (int i = 0; i < 5; i++) Console.WriteLine();
                Console.WriteLine(Animation.DOOR_P); Thread.Sleep(1000);
                for (int i = 0; i < 8; i++) Console.WriteLine();

                Console.Clear();
                for (int i = 0; i < 10; i++) Console.WriteLine();
                Typing.AnimateType("Oh... It's just a dead-end...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                Typing.AnimateType("\n\nI swear I saw a path ahead earlier but... how do I even cross this wall?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                Typing.AnimateType("\n\nI could try to climb but the ladder's broken. The boxes on the ground doesn't look that stable too...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t \u001b[5mWill you attempt to climb the wall? (Y/N)  \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    if (choice == "y" || choice == "Y")
                    {
                        for (int i = 0; i < 25; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to climb the wall");
                        saveSystem.SaveCheckpoint("WallChoice", choice);
                        Continue.ContinueOrExit();

                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        int startY = Console.CursorTop;
                        int frameHeight = Animation.PRE_CLIMB[0].Split('\n').Length;
                        int safeY = Typing.GetSafeYOffset(startY, frameHeight);
                        Typing.AnimateFrames(Animation.PRE_CLIMB, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                        for (int i = 0; i < 27; i++) Console.WriteLine();
                        Typing.AnimateType("Ouch that looks painful...", ConsoleColor.Green, "center"); Thread.Sleep(400);

                        for (int i = 0; i < 8; i++) Console.WriteLine();
                        bool finished = false;

                        while (!finished)
                        {
                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write("\n\n\t\t\t\t   \u001b[5m Will you still push through and climb the wall[1] or will you walk away instead[2]?\u001b[0m"); Thread.Sleep(400);
                            Console.Write("\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                            string? choose = Console.ReadLine();
                            Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "An option is given");

                            if (choose == "1")
                            {
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> still choose to climb the wall");

                                for (int i = 0; i < 30; i++) Console.WriteLine();
                                startY = Console.CursorTop;
                                frameHeight = Animation.CLIMB[0].Split('\n').Length;
                                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                Typing.AnimateFrames(Animation.CLIMB, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);

                                for (int i = 0; i < 25; i++) Console.WriteLine();
                                Typing.AnimateType("No! I was almost there!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                                for (int i = 0; i < 5; i++) Console.WriteLine();
                                while (true)
                                {
                                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                    color.TEXT("\n\n\u001b[5m You almost got it but still fell... Much painful this time too...\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                                    color.TEXT("\n\u001b[5m Maybe if you give it another try, I'm sure things will work out!\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                                    color.TEXT("\n\u001b[5m Don't give up just yet! So- will you give it your all this time and climb that wall?\u001b[0m", ConsoleColor.DarkYellow); Thread.Sleep(400);
                                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice(Y/N): \u001b[0m");
                                    string? choose1 = Console.ReadLine();
                                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                    Thread.Sleep(2000);
                                    historyLog.QuickLog("SYSTEM", "An option is given");

                                    if (choose1 == "y" || choose1 == "Y")
                                    {
                                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> still choose to climb the wall");
                                        for (int i = 0; i < 30; i++) Console.WriteLine();
                                        startY = Console.CursorTop;
                                        frameHeight = Animation.SUCCESS[0].Split('\n').Length;
                                        safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                        Typing.AnimateFrames(Animation.SUCCESS, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully crossed over the wall!");

                                        for (int i = 0; i < 30; i++) Console.WriteLine();
                                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                        Console.Write($"\n\n\t\t\t\t\t\t\t\t     \u001b[5mWow, great job! \u001b[0m");
                                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                        Thread.Sleep(2000);
                                        finished = true; break;
                                    }

                                    else if (choose1 == "n" || choose1 == "N")
                                    {
                                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to give up climbing the wall");
                                        for (int i = 0; i < 5; i++) Console.WriteLine();
                                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                        Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m DONT'T GIVE UP! \u001b[0m");
                                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                        Thread.Sleep(2000);
                                        for (int i = 0; i < 15; i++) Console.WriteLine();
                                        historyLog.QuickLog("SYSTEM", "I wont allow them, of course. They're so near.");

                                        startY = Console.CursorTop;
                                        frameHeight = Animation.AGAIN[0].Split('\n').Length;
                                        safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                        Typing.AnimateFrames(Animation.AGAIN, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                                        for (int i = 0; i < 30; i++) Console.WriteLine();
                                        startY = Console.CursorTop;
                                        frameHeight = Animation.SUCCESS[0].Split('\n').Length;
                                        safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                        Typing.AnimateFrames(Animation.SUCCESS, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1000);
                                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully crossed over the wall!");
                                   
                                        for (int i = 0; i < 30; i++) Console.WriteLine();
                                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                                        Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5mWow, great job! \u001b[0m");
                                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                                        Thread.Sleep(2000);
                                        finished = true; 
                                        break;
                                    }

                                    else
                                    {
                                        Console.Clear();
                                        historyLog.QuickLog("SYSTEM", "ERROR");
                                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                                        Thread.Sleep(1000);
                                        Console.Clear();
                                    }
                                }
                            }

                            else if (choose == "2")
                            {
                                for (int i = 0; i < 30; i++) Console.WriteLine();
                                startY = Console.CursorTop;
                                frameHeight = Animation.CLIMB[0].Split('\n').Length;
                                safeY = Typing.GetSafeYOffset(startY, frameHeight);
                                Typing.AnimateFrames(Animation.FAIL, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to stop climbing the wall");

                                Console.Write("\x1b[3J");
                                Console.Clear();
                                Console.SetCursorPosition(0, 0);
                                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                                historyLog.QuickLog("SYSTEM", "<<PLAYER>> walks away from the wall");
                                for (int i = 0; i < 10; i++) Console.WriteLine();
                                Console.WriteLine(Animation.OBSTACLE); for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(400);
                                for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000);

                                Trial1_door trial = new Trial1_door(new playerDetails());
                                trial.Again();
                                finished = true; break;
                            }

                            else
                            {
                                Console.Clear();
                                historyLog.QuickLog("SYSTEM", "ERROR");
                                color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                                Thread.Sleep(1000);
                                Console.Clear();
                            }
                        }
                        break;
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to climb the wall");
                        saveSystem.SaveCheckpoint("WallChoice", choice);
                        Continue.ContinueOrExit();

                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        int startY = Console.CursorTop;
                        int frameHeight = Animation.FAIL[0].Split('\n').Length;
                        int safeY = Typing.GetSafeYOffset(startY, frameHeight);
                        Typing.AnimateFrames(Animation.FAIL, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> walks away from the wall");
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OBSTACLE); for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(2000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(400);

                        Trial1_door trial = new Trial1_door(new playerDetails());
                        trial.Again();
                        break;
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
                Trial2 trial2 = new Trial2(_player);
                trial2.PlayAnimation();
            }
        }

        public Door1 DoorOne { get; }
        public Door2 DoorTwo { get; }
        public Door3 DoorThree { get; }
        public Door4 DoorFour { get; }
        public Door5 DoorFive { get; }
        public Door6 DoorSix { get; }

        public Trial1()
        {
            DoorOne = new Door1();
            DoorTwo = new Door2();
            DoorThree = new Door3();
            DoorFour = new Door4();
            DoorFive = new Door5();
            DoorSix = new Door6();
        }

        public class Door1()
        {
            HistoryLog historyLog = new HistoryLog();
            ColoredText color = new ColoredText();
            Continue saveSystem = new Continue();

            public void End2() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the CHEF");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                             "\n\n\n\n\n\tBrandon Mull >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t\t 'Indulgence is emptiness. I have proved the limits of food and frivolity...", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t you end up feeding without being nourished.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                for (int i = 0; i < 4; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door1Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered a resturant");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the CHEF");

                for (int i = 0; i < 5; i++) Console.WriteLine();

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.CHEF, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.PIE);
                for (int i = 0; i < 5; i++) Console.WriteLine();
                Console.Clear();
                Typing.AnimateType("\n\n\t\t\t   CHEF: Hello child! Wow, I'm very happy you chose this resturant!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   CHEF: I'll be your chef today, tomorrow, and forver and ever!!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   CHEF: I can serve everything your heart desired the most!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   CHEF: Though, the once in the display is what we'll only be serving for today hehe~", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   CHEF: If you follow me, you'll always experience the PLEASURE IN FOODS for the rest of your life!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\n\t\t\t   CHEF: Sounds tempting right?? Right~??", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t      \u001b[5mWill you follow the CHEF (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choice == "y" || choice == "Y")
                    {
                        saveSystem.SaveCheckpoint("End2", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the CHEF");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\t\tBrandon Mull >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'Indulgence is emptiness. I have proved the limits of food and frivolity...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t you end up feeding without being nourished.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the CHEF");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 8; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME1); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End2", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\t\tBrandon Mull >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'Indulgence is emptiness. I have proved the limits of food and frivolity...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t you end up feeding without being nourished.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }

        public class Door2()
        {
            HistoryLog historyLog = new HistoryLog();
            ColoredText color = new ColoredText();
            Continue saveSystem = new Continue();

            public void End3() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the INN KEEPER");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tAnthony Liccione >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t\t 'Let the night take you, Let the stars evaporate into your dreams.", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\tLet sleep be the only comfort for you to believe.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                for (int i = 0; i < 3; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door2Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered an inn");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the INN KEEPER");
                for (int i = 0; i < 5; i++) Console.WriteLine();

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.KEEPER, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 15; i++) Console.WriteLine();
                Console.WriteLine(Animation.BED);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Typing.AnimateType("\n\n\t\t\t   INN KEEPER: Oh hello there! You must be new here! Please, please, do come in!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   INN KEEPER: I presume you're here to rest from your journey?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   INN KEEPER: You have nothing to worry about-- we have the most comfortable bed you'll ever find!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   INN KEEPER: You'll never have to worry about anything else, all to your heart's content.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   INN KEEPER: If you follow me, you'll get to REST for the rest of your life!.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\n\t\t\t   INN KEEPER: Sounds tempting right?? Right~??", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWill you follow the INN KEEPER: (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choice == "y" || choice == "Y")
                    {
                        saveSystem.SaveCheckpoint("End3", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the INN KEEPER");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tAnthony Liccione >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'Let the night take you, Let the stars evaporate into your dreams.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\tLet sleep be the only comfort for you to believe.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 3; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the INN KEEPER");
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 8; i++) Console.WriteLine();

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME2); Thread.Sleep(100);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End3", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\n\t\tAnthony Liccione >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'Let the night take you, Let the stars evaporate into your dreams.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\tLet sleep be the only comfort for you to believe.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 3; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }

        public class Door3()
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void End4() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the GAMER");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tOrson Scott Card >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t\t 'I hope you had fun, I hope you had a nice, nice time being happy.", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\tIt might be the last time in your life.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                for (int i = 0; i < 5; i++) Console.WriteLine();
                Typing.AnimateType("'The game ends when you YOU stop playing.'", ConsoleColor.Green, "center");

                for (int i = 0; i < 1; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door3Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered a unique computer shop");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the GAMER");

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.GAMER, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.CONSOLE);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Typing.AnimateType("\n\n\t\t\t   GAMER: Oh hi there... Uhh what can I help you with?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   GAMER: No wait, you're here to play games too right? Well, you're in the right place!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   GAMER: Please, do come in and make yourself comfortable. We got everything your heart desires.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   GAMER: It's all for free too! Hurry hurry, I need to go back to the game, my friends are waiting!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\n\t\t\t   GAMER: Sounds tempting right?? Right~??", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWill you follow the GAMER: (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choose = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choose == "y" || choose == "Y")
                    {
                        saveSystem.SaveCheckpoint("End4", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the GAMER");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tOrson Scott Card >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'I hope you had fun, I hope you had a nice, nice time being happy.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\tIt might be the last time in your life.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Typing.AnimateType("'The game ends when you YOU stop playing.'", ConsoleColor.Green, "center");

                        for (int i = 0; i < 1; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choose == "n" || choose == "N")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the GAMER");
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 6; i++) Console.WriteLine();

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME3); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End4", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\n\t\tOrson Scott Card >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'I hope you had fun, I hope you had a nice, nice time being happy.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\tIt might be the last time in your life.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        for (int i = 0; i < 3; i++) Console.WriteLine();
                        Typing.AnimateType("'The game ends when you YOU stop playing.'", ConsoleColor.Green, "center");
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 2; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }

        public class Door4()
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void End5() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the STRANGER");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\t\tLacey L. >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t 'The worst part about anything self-destructive is that it's so intimate.", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t You become so close with your addictions and illnesses that leaving them behind...", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t is like killing the part of yourself that taught you how to survive.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 4; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");


                for (int i = 0; i < 4; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door4Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered a very messy room");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the STRANGERR");

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.STRANGER, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.CIGAR);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Typing.AnimateType("\n\n\t\t\t   STRANGER: Heh heheh. Oh~ look what we have here~ A newbie.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   STRANGER: Cough. cough. cough.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   STRANGER: So wanna try 'em out? They're pretty refreshing if you ask me. Fruity too~ Want some?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   STRANGER: I have everything you need. Pills? Herbs? Sparkling Drinks?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   STRANGER: Cough. cough. cough.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   STRANGER: Sounds tempting right?? Right~?? Heh. Come in, come in, I've got more of my collections~", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWill you follow the STRANGER: (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choice == "y" || choice == "Y")
                    {
                        saveSystem.SaveCheckpoint("End5", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the STRANGER");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\t\tLacey L. >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t 'The worst part about anything self-destructive is that it's so intimate.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t You become so close with your addictions and illnesses that leaving them behind...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t is like killing the part of yourself that taught you how to survive.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 4; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");


                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the STRANGER");
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 8; i++) Console.WriteLine();

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME4); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End5", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\t\tLacey L. >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t 'The worst part about anything self-destructive is that it's so intimate.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t You become so close with your addictions and illnesses that leaving them behind...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t is like killing the part of yourself that taught you how to survive.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }

        public class Door5()
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void End6() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the HOST");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tNeil Postman >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("'We amuse ourselves to death'", ConsoleColor.Red, "center");
                for (int i = 0; i < 6; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                for (int i = 0; i < 4; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door5Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered an mysterious huge space");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the HOST");

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.HOST, repeat: 1, delay: 1000, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.DRINK);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Typing.AnimateType("\n\n\t\t\t   HOST: Hey there! Want to join me and my friends?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   HOST: We're having so much fun here, as you can see. Here, you can party all you want!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   HOST: You can play any esports or sports all you want!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   HOST: Heck yeah, we even have books, movies, shows, and forms of music in our special corner!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   HOST:  Got some cool talents to share? Drawing? Animating? Editing? You chose the right place!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   HOST:  Our room is much bigger than you think, all to satisfy you. ", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\n\t\t\t   HOST: Sounds tempting right?? Right~?? Come in, come in!", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t   \u001b[5mWill you follow the HOST: (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choice == "y" || choice == "Y")
                    {
                        saveSystem.SaveCheckpoint("End6", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the HOST");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tNeil Postman >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("'We amuse ourselves to death'", ConsoleColor.Red, "center");
                        for (int i = 0; i < 6; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the HOST");
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 8; i++) Console.WriteLine();

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME5); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End6", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\t\tNeil Postman >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n'We amuse ourselves to death'", ConsoleColor.Red, "center");
                        for (int i = 0; i < 6; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }

        public class Door6()
        {
            ColoredText color = new ColoredText();
            HistoryLog historyLog = new HistoryLog();
            Continue saveSystem = new Continue();

            public void End7() //end
            {
                Console.Clear();
                Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
                Console.Clear();

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the SPECIALIST");
                Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tJR >>", blinkcnt: 2, on: 500, off: 300);
                Typing.AnimateType("\n\n\t\t\t 'The more social media we have, the more we think we're connecting...", ConsoleColor.Red, "left");
                Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t yet we are really disconnecting from each other.'", ConsoleColor.Red, "left");
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                for (int i = 0; i < 4; i++) Console.WriteLine();
                Console.WriteLine(Animation.bye);
                Environment.Exit(0);
            }

            public void Door6Scene()
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Green);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> discovered they've entered an cute office");
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> meets the SPECIALIST");

                int startY = Console.CursorTop;
                int safeY = Math.Min(startY, Console.BufferHeight - 8);
                Typing.AnimateFrames(Animation.SPECIALIST, repeat: 1, delay: 1500, yOffset: safeY); Thread.Sleep(1500);

                for (int i = 0; i < 10; i++) Console.WriteLine();
                Console.WriteLine(Animation.PHONE);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: Oh? Sorry, I didn't notice you there! I've been chatting with my friends.", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: Come here, come here. Take a seat. I've got a lot of things I want to show you!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: New trends? New updates? New tea? New communities you want to join?", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: In this place, it'll be like having everything in the world at your hands!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: Oh, I’m sure you want to raise your digital reputation, too!", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\t\t\t   SPECIALIST: No worries, I'll be there to guide you through it all.  ", ConsoleColor.Red, "left"); Thread.Sleep(500);
                Typing.AnimateType("\n\n\n\t\t\t   SPECIALIST: Sounds tempting right?? Right~?? So? Wanna join me?", ConsoleColor.Red, "left"); Thread.Sleep(500);

                for (int i = 0; i < 5; i++) Console.WriteLine();
                while (true)
                {
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t \u001b[5mWill you follow the SPECIALIST: (Y/N)?  \u001b[0m");
                    Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                    string? choice = Console.ReadLine();
                    Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "An option is given");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);

                    if (choice == "y" || choice == "Y")
                    {
                        saveSystem.SaveCheckpoint("End7", "");
                        Continue.ContinueOrExit();
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to follow the SPECIALIST");
                        Typing.Blink("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n" +
                                     "\n\n\n\n\n\n\t\tJR >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'The more social media we have, the more we think we're connecting...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t yet we are really disconnecting from each other.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> ended their journey so short");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else if (choice == "n" || choice == "N")
                    {
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to follow the SPECIALIST");
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n", ConsoleColor.Red);
                        historyLog.QuickLog("SYSTEM", "Opps, <<PLAYER>> made the wrong choice indeed");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.ERROR); Thread.Sleep(1000);
                        for (int i = 0; i < 8; i++) Console.WriteLine();

                        color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\n\u001b[5m YOU ARE UNABLE TO REFUSE AT THIS STAGE. YOU HAVE NO OTHER OPTION.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT($"\n\u001b[5m YOU SHOULD CHOSEN WISELY AND NOT LOSE SIGHT OF YOUR PATH.  \u001b[0m", ConsoleColor.Red);
                        color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.Red);
                        Thread.Sleep(2000);

                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.WELCOME6); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(5000);

                        saveSystem.SaveCheckpoint("End7", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\n\t\tJR >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t 'The more social media we have, the more we think we're connecting...", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t yet we are really disconnecting from each other.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> will have to end their journey whether they like it or not");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Not the Right Time' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }

                    else
                    {
                        Console.Clear();
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        Console.Clear();
                    }
                }
            }
        }
    }
     
    public class Checkpoint : playerDetails 
    {
        ColoredText color = new ColoredText();
        HistoryLog historyLog = new HistoryLog();
        Continue saveSystem = new Continue();

        public void SaveCheckpoint(string choose)
        {
            saveSystem.SaveCheckpoint("Checkpoint", choose);
        }
      
        public void ContinuePOINT(string choose) //continue
        {
            Console.Clear();
            Console.WriteLine($"Resuming to Checkpoint from Choice {choose}..."); Thread.Sleep(2000);
            Console.Clear();
            int safeY;

            while (true)
            {
                if (choose == "1")
                {
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to take a rest. Not to confine on anyone even if it's a system like me.");
                    saveSystem.SaveCheckpoint("Checkpoint", choose);
                    Continue.ContinueOrExit();

                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\t\t\u001b[5m    Hmm...? \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m You say you're 'fine' but I know what that really means. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(4000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                    Typing.AnimateType("\n\n'The worst kind of pain is when you're smiling just to stop the tears from falling.'", ConsoleColor.Red, "center");
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m      I wish you luck on your journey. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m    Thanks for stopping by the Checkpoint. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(3000);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    Console.Clear();
                    for (int i = 0; i < 15; i++) ;
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.PRE_TRIAL3[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.PRE_TRIAL3, repeat: 1, delay: 500, yOffset: safeY); Thread.Sleep(1500);

                    Trial3 trial3 = new Trial3();
                    trial3.PlayAnimation();
                    break;
                }

                else if (choose == "2" || choose == "3")
                {
                    saveSystem.SaveCheckpoint("Checkpoint", choose);
                    Continue.ContinueOrExit();
                    string filePath = @"C:\Users\Tiffany Mae\Documents\game\entry.txt";
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to take a break and chat with the system");
                    string date = DateTime.Now.ToString("MM-dd-yyyy");

                    string oldContent = "";
                    if (File.Exists(filePath))
                    {
                        oldContent = File.ReadAllText(filePath);
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.BOTH("Previous Entries:\n", ConsoleColor.Black, ConsoleColor.Green);
                        Console.WriteLine(oldContent);
                        Console.WriteLine("\n\nNew Entry:\n");
                    }

                    Console.Write("\t> ");
                    string? entry = Console.ReadLine();

                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        if (!string.IsNullOrWhiteSpace(oldContent))
                        {
                            writer.WriteLine(oldContent.TrimEnd());
                            writer.WriteLine();
                        }

                        writer.WriteLine("Checkpoint Entries - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                        writer.WriteLine($"\n\t{DateTime.Now:MM-dd-yyyy}");
                        writer.WriteLine("\n\n• PLAYER: ");
                        writer.WriteLine($"\t> {entry}\n\n\n");
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> submitted their entry in 'C:\\Users\\Tiffany Mae\\Documents\\game\\entry.txt'");
                    }

                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\u001b[5m Checkpoint Entry Saved Successfully \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "Checkpoint Entry Saved Successfully");

                    while (true)
                    {
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\t\u001b[5m Do you wish to check your entry? (Y/N) \u001b[0m");
                        Console.Write($"\n\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                        string? option = Console.ReadLine();
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "An option is given");

                        if (option == "y" || option == "Y")
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });

                            for (int i = 0; i < 6; i++) Console.WriteLine();
                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m   Checkpoint File Opened Successfully \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Checkpoint File Opened Successfully");
                            break;
                        }

                        else if (option == "n" || option == "N") break;

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Console.Clear();
                        }
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m   I wish you luck on your journey. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m Thanks for stopping by the Checkpoint. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(3000);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    for (int i = 0; i < 4; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < 15; i++) ;
                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.PRE_TRIAL3[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.PRE_TRIAL3, repeat: 1, delay: 500, yOffset: safeY); Thread.Sleep(1500);
                    Console.Clear();
                    Trial3 trial3 = new Trial3();
                    trial3.PlayAnimation();
                    break;
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Console.Clear();
                }
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
            }
        }

        public void POINT() //newgame
        {
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
            for (int i = 0; i < 5; i++) Console.WriteLine();
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the Checkpoint Area");

            Console.WriteLine(Animation.OLD); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            color.TEXT("Hm? What's this?? I hope it's nothing dangerous...", ConsoleColor.DarkYellow);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write("\n\n\t\t\t\t\t\t\t\t\t \u001b[5m    !! \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(4000);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write("\n\n\t\t\t\t\t\t\t\t\t \u001b[5m    !! \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(4000);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            while (true)
            {
                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\u001b[5m     Hello there player {userName}! Welcome to the Checkpoint! \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\u001b[5m The journey you walk is long and endless, even the stars need time to rest. \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t\t\t\u001b[5m  So before you continue... \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t\t\t\u001b[5mHow are you feeling right now? \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t   \u001b[5m    You don't have to hide it--  I'm here to listen. \u001b[0m");
                Console.Write("\n\n\n\t\t\t\t\t\t \u001b[5m 1) I'm fine, really. Nothing to worry about... \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 2) Even though I don't want to admit it... I'm not okay. \u001b[0m");
                Console.Write("\n\n\t\t\t\t\t\t \u001b[5m 3) I don't know how I feel. Just... weird, I guess? \u001b[0m");
                Console.Write($"\n\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                string? choose = Console.ReadLine();
                Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);
                historyLog.QuickLog("SYSTEM", "An option is given");

                for (int i = 0; i < 10; i++) Console.WriteLine();

                if (choose == "1")
                {
                    saveSystem.SaveCheckpoint("CheckpointChoice", choose);
                    Continue.ContinueOrExit();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose not to take a rest. Not to confine on anyone even if it's a system like me.");

                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\t\t\u001b[5m  Hmm...? \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t   \u001b[5m You say you're 'fine' but I know what that really means. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(4000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                    Typing.AnimateType("\n\n'The worst kind of pain is when you're smiling just to stop the tears from falling.'", ConsoleColor.Red, "center");
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m     I wish you luck on your journey. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m  Thanks for stopping by the Checkpoint. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(3000);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 4; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(800);
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < 15; i++) ;
                    int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.PRE_TRIAL3[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.PRE_TRIAL3, repeat: 1, delay: 500, yOffset: safeY); Thread.Sleep(1500);
                    Trial3 trial3 = new Trial3();
                    trial3.PlayAnimation();
                    break;
                }

                else if (choose == "2" || choose == "3")
                {
                    saveSystem.SaveCheckpoint("CheckpointChoice", choose);
                    Continue.ContinueOrExit(); ;
                    string filePath = @"C:\Users\Tiffany Mae\Documents\game\entry.txt";
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to take a break and chat with the system");
                    string date = DateTime.Now.ToString("MM-dd-yyyy");

                    string oldContent = "";
                    if (File.Exists(filePath))
                    {
                        oldContent = File.ReadAllText(filePath);
                        historyLog.QuickLog("SYSTEM", "ERROR");
                        color.BOTH("Previous Entries:\n", ConsoleColor.Black, ConsoleColor.Green);
                        Console.WriteLine(oldContent);
                        Console.WriteLine("\n\nNew Entry:\n");
                    }

                    Console.Write("\t> ");
                    string? entry = Console.ReadLine();

                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        if (!string.IsNullOrWhiteSpace(oldContent))
                        {
                            writer.WriteLine(oldContent.TrimEnd());
                            writer.WriteLine();
                        }

                        writer.WriteLine("Checkpoint Entries - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                        writer.WriteLine($"\n\t{DateTime.Now:MM-dd-yyyy}");
                        writer.WriteLine("\n\n• PLAYER: ");
                        writer.WriteLine($"\t> {entry}\n\n\n");
                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> submitted their entry in 'C:\\Users\\Tiffany Mae\\Documents\\game\\entry.txt'");
                    }

                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m    Checkpoint Entry Saved Successfully \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(2000);
                    historyLog.QuickLog("SYSTEM", "Checkpoint Entry Saved Successfully");

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    while (true)
                    {
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m   Do you wish to check your entry? (Y/N) \u001b[0m");
                        Console.Write($"\n\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                        string? option = Console.ReadLine();
                        Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "An option is given");

                        if (option == "y" || option == "Y")
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });

                            for (int i = 0; i < 6; i++) Console.WriteLine();
                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m   Checkpoint File Opened Successfully \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Checkpoint File Opened Successfully");
                            break;
                        }

                        else if (option == "n" || option == "N") break;

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Console.Clear();
                        }
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m      I wish you luck on your journey. \u001b[0m");
                    Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m    Thanks for stopping by the Checkpoint. \u001b[0m");
                    Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                    Thread.Sleep(3000);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++) ;
                    int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.PRE_TRIAL3[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.PRE_TRIAL3, repeat: 1, delay: 500, yOffset: safeY); Thread.Sleep(1500);
                    Trial3 trial3 = new Trial3();
                    trial3.PlayAnimation();
                    break;
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Console.Clear();
                }
                historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");
            }
        }
    }

    internal class Trial2 : NewGame, IAnimation, IChecks
    {
        HistoryLog historyLog = new HistoryLog();
        ColoredText color = new ColoredText();
        AgeDefiner ageChecker = new AgeDefiner();
        int ageValue;
        private playerDetails currentPlayer;
        Continue saveSystem = new Continue();

        public void SaveCheckpoint(string choice)
        {
            saveSystem.SaveCheckpoint("Trial2", choice);
        }

        public Trial2(playerDetails player)
        {
            currentPlayer = player;
            ageValue = playerDetails.age;
        }

        public void End8() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\t\t\t\t\t 'Retirement is not the end of the road;", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t it is the beginning of the open highway.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Retirement' Ending Route Discovered");

            for (int i = 0; i < 4; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void End9() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\n\n\t\tVictoria Erickson >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\t\t 'Sometimes the end of a journey isn't a place on a map, but the moment your heart tells you it's time to stay.", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\tEspecially when love — even the kind with four paws — plants its roots.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Mourning' Ending Route Discovered");

            for (int i = 0; i < 4; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void ContinueAnimation(string choice) //continue
        {
            Console.Clear();
            Console.WriteLine($"Resuming the Second Trial from Choice {choice}..."); Thread.Sleep(2000);
            Console.Clear();
            int safeY;

            while (true)
            {
                if (choice == "1")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go with PAL");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow); Thread.Sleep(400);
                    color.TEXT("\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n\n...So you won't mind if I follow you this time, right <Pal>?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'll be the companion you've been looking for...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\n\t\t\t   PAL: *Gasp!! N-no way!", ConsoleColor.Magenta); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: I don't mind at all. I'm happy to have you by my side, too!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Together, we can explore new places, experience difficulties along the way...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: maybe a fight or two, but we'll be fine as long as we're together...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_HI); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\nHey buddy, I'll miss you very much too. Are you sure you won't follow us?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nTogether we'll be the greatest team that'll ever be...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.HUG); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\nGoodbye <Athan>, I hope you'll meet new friends in your journey too...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(900);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> and PAL continues their journey");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green); historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to use the MAKESHIFT RAFT");
                    historyLog.QuickLog("SYSTEM", "TIMESKIP TO 10 YEARS");
                    historyLog.QuickLog("SYSTEM", "Things aren't exactly the same anymore");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nDays turned to Weeks.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWeeks turned to Months.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nAnd Months turned to Years.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nOh, look, ten years have now passed?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYet the two companions never separated. They were tight as a glue!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey had distinct and unique personalities, but this only brought them closer as they learn from each other.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nFriendship bloomed, tighter than a knot.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey only have each other to rely on. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nHowever, even such a beautiful friendship can never avoid conflicts. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nConflict that hurt like a twisting knife. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nConflict that could forever damage a relationship, even if it's only a simple mistake. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nLike a wise man once said, 'trust takes years to build, seconds to break, and forever to repair'... ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n'<Pal> Please, you can't do this to me! We can still make it right...!' ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nGuilt that swallows the poor child whole ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nRegrets haunted the child every day. They once again felt loneliness they haven't felt for years...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m"); ;
                        Console.Write($"\n\n\t\t\t\t\u001b[5m 1) Cling to them and do whatever it takes to make things right  \u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\u001b[5m 2) Let them go. They are tired. You are tired. You will only drain each other dry.   \u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                        string? choose = Console.ReadLine();
                        Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "An option is given");
                        if (choose == "1")
                        {
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(600);
                            }
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to cling to PAL");
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\n\nOh, what a toxic relationship this has turned to be.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nFriendship that's preserved by locks and chains.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe child only kept clinging!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nHowever, things can never be the same again.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nTheir once-best-ever-friend now looks at them with contempt and distrust", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe child knew this would happen-- ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nBut their friend was the only person they've ever known since coming to this world!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nSo they cling and CLING and eventually, they were pushed away, never to be seen again.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nOh, what a tragic ending this has turned out to be.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000); break;
                        }

                        else if (choose == "2")
                        {
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(600);
                            }
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to let go of PAL");
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\n\nThe child made a big sacrifice.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nIt's never easy to let someone go.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nBut it's something that must be done eventually.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nWhy hold on to a relationship that could only destroy you and kill you slowly, right?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nSeparation is painful, but sometimes it's what's best for everyone.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nIf you love them, you let them go, right?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nChin up, things will be okay.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000);
                            break;
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Console.Clear();
                        }
                    }

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    if (ageValue >= 10 && ageValue <= 20)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> slowly continues their journey again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 21 && ageValue <= 40)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT_SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT_SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT, repeat: 1, delay: 1200, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> slowly continues their journey again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 41 && ageValue <= 50)
                    {
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OLD); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "'???' switched from Narration Mode to Invisible Character Mode");

                        Typing.AnimateType("\n\nMy child, look how old you are now...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\n\n\nWho...? Hmm but yea... I don't think I can continue any longer.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to stay here, maybe build a house or something.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nThere are a lot of things life can still offer.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to settle here and start life anew.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);

                        for (int i = 0; i < 9; i++) Console.WriteLine();
                        Typing.AnimateType($"\t\t\t\t\t\t\t\t I see. It's great knowing you <{currentPlayer.userName}>", ConsoleColor.Green); Thread.Sleep(1500);

                        saveSystem.SaveCheckpoint("End8", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t\t\t 'Retirement is not the end of the road;", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t it is the beginning of the open highway.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Retirement' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        return;
                    }
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else if (choice == "2")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go with ATHAN");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow);
                    color.TEXT("\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n...So I won't say goodbye, my friend.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIf it's our destiny to meet again, we'll surely find each other when the time is right.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm gonna miss you <Pal>! Take care!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.LOW); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\n\t\t\t   PAL: Aww, I will miss you too, my dear friend!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Thank you for letting me be a part of your journey. Until then...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nI guess it's you and I now, <Athan>", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI know you're also going to a separate way but... you wouldn't mind if I accompany you, right?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI never thought I would hate being alone again.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.HUG); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nAwww~ I'll take that as a yes then hehee...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nFrom now on, you'll be my family! A parent, a sibling, or even my child!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Typing.AnimateType("\t\t\t\t\t\t\t\t\t   ATHAN: Awwooooooo! Awoooo!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> and ATHAN continues their journey");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "Timeskip to 10 years");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nDays turned to Weeks.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWeeks turned to Months.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nAnd Months turned to Years.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYears passed by so quickly...  I see that it's been 10 years already?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut despite the language barriers and species differences, neither of them seemed to mind at all.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nNever were they separated. Such loyalty, such love!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe presence of one another helped fill the gap of the trio they once were.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut slowly, despite being so close, you didn't know how <Athan> was feeling.. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWhat <Athan> was experiencing. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey're getting weaker as days pass by. They hardly eat at all, always sleeping in your lap.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYou know something's happening, but you can't quite understand what's happening. Until..", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\n'<Athan!> Where are you, buddy? Please make any noise so I can find you!'", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n'Please tell me we’re just playing hide and side-- Come on, where are you?'", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\nIt's been hours since you've last seen them. They disappeared without any trace!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe food, water, and toys were all untouched.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n'Were they kidnapped?', you thought for a second.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\nNot until you saw them, peacefully sleeping...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_DIE); Thread.Sleep(1000);
                    for (int i = 0; i < 8; i++) Console.WriteLine();

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> found ATHAN dead");

                    Typing.AnimateType("\n\n'Buddy? You scared me! Were you tire--'", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nNo heartbeat. No gentle breaths you are used to. ", ConsoleColor.Red, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> IS DEAD.", ConsoleColor.Red, "center"); Thread.Sleep(400);

                    Typing.AnimateType("\n\n\n\n\n\nNo, they're just sleeping, I'm sure!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\n\nNo, they're dead. And you haven't realized that all this time, he was dying.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> left you; they knew they were dying today.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey knew this would only hurt you.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe child... You knew this day was coming.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYou knew something was going on! Yet you didn't do anything!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("Oh, dear <Athan>... why didn't you tell me?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI could have stayed by your side and let you feel safe.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI could have showered you with enough warmth until your last breath...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "'???' switched from Narration Mode to Invisible Character Mode");

                    Typing.AnimateType("\n\nMy dear child, please rise, you still need to continue your journey.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> has already finished their role, their journey in this world.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nLive for him. Live for his sake.", ConsoleColor.Green, "center"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\n\n\n\nGoodbye <Athan>...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(5000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    if (ageValue >= 10 && ageValue <= 20)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.BURIAL[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.BURIAL, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 15; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY);
                        for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey, this time, alone again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 21 && ageValue <= 40)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.BURIAL, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT_SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT_SAD, repeat: 1, delay: 600, yOffset: safeY);
                        for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(1000);

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT, repeat: 1, delay: 1200, yOffset: safeY);
                        for (int i = 0; i < 105; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey, this time, alone again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 41 && ageValue <= 50)
                    {
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OLD);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Typing.AnimateType("\n\nMy child, look how old you are now...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\n\n\n Please, can't I stay here together with them?", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI'm already old, I can't handle the long journey ahead.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nPlus, I'm not used to traveling alone anymore...", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to settle here and start life anew. I just want to rest already.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nMaybe make a house or something...", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nAnything... just so that can't leave this place behind.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Typing.AnimateType($"I see. It's great knowing you <{currentPlayer.userName}>", ConsoleColor.Green, "center"); Thread.Sleep(1500);

                        saveSystem.SaveCheckpoint("End9", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\n\t\tVictoria Erickson >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t 'Sometimes the end of a journey isn't a place on a map, but the moment your heart tells you it's time to stay.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\tEspecially when love — even the kind with four paws — plants its roots.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Mourning' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else if (choice == "3")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to continue their journey alone");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow);
                    color.TEXT("\n\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n...So I won't say goodbye, my friend.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIf it's our destiny to meet again, we'll surely find each other when the time is right.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm gonna miss you <Pal>! Take care!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.LOW); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\n\t\t\t   PAL: Aww, I will miss you too, my dear friend!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Thank you for letting me be a part of your journey. Until then...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("Hey buddy... Are you sure you're okay being alone?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nHm... Is that so... I guess I'll take that as a yes. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI don't know where you're going, but I pray for your safe travels.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI hope you'll meet a good friend or two on your journey, too. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_HI); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nI will miss you, buddy, I won't say goodbye like I did for <Pal>", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n...I hope our paths interconnect again. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nHm...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nWow... I never thought I'd be alone again in this journey.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIt feels so wrong without anyone to talk to... anyone present near me.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut I want to respect their decisions, I know that eventually we'll end up separating.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm used to this so.. I should be fine...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Thread.Sleep(1000);
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Console.Clear();
                }
            }
        }
        
        public void PlayAnimation() //newgame
        {
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the 2nd Trial");
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Typing.AnimateType("\n\t\t\t\tHmm~ Hmm~ Whoever can string my husband's old bow~ Oh~ Waiting~", ConsoleColor.DarkYellow); Thread.Sleep(400);
            Typing.AnimateType("\n\n\t\t\t\t\t\tHmm~ Let the arrow fly when you know that your aim is true~~", ConsoleColor.DarkYellow); Thread.Sleep(400);
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\tHmm~ Let the arrow fly when you know that your aim is true~~", ConsoleColor.DarkYellow); Thread.Sleep(500);

            for (int i = 0; i < 5; i++) Console.WriteLine();
            int safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.start[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.start, repeat: 2, delay: 500, yOffset: safeY); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.startwalk[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.startwalk, repeat: 5, delay: 500, yOffset: safeY); Thread.Sleep(1000);
            for (int i = 0; i < 13; i++) Console.WriteLine();

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> encounters an interesting obstacle. It was the people.");

            Typing.AnimateType("\n\t\t\t\t\t\tHuh? There are also other people here? I wonder who they are?", ConsoleColor.DarkYellow); Thread.Sleep(600);
            Typing.AnimateType("\n\n\n\t\t\t\t\t\t\t\t*cough *cough UHMM HELLO??", ConsoleColor.DarkYellow); Thread.Sleep(600);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.PEOPLE); Thread.Sleep(1000);
            for (int i = 0; i < 15; i++) Console.WriteLine();

            safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.FRIEND[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.FRIEND, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.FRIEND_HI); Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) Console.WriteLine();

            safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.DOG[0].Split('\n').Length);
            Typing.AnimateFrames(Animation.DOG, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.DOG_HI); Thread.Sleep(1000);

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.PEOPLE_HI); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            historyLog.QuickLog("SYSTEM", "The three characters met");

            Typing.AnimateType("\n\n\t\t\t   ??: I'm very happy I'm not alone in this journey! You really came at the right time!", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   ??: I've always been dreaming of a companion to accompany me. And here you are! Pleasee~", ConsoleColor.Magenta); Thread.Sleep(500);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.FRIEND_HI); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            Typing.AnimateType("\n\n\t\t\t   ??: I don't mind if it's just for a while, I don't mind if it's for a long time too~~", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   ??: Oh, I almost forgot, you can call me <Pal>! You don't mind me joining you, right??", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t   <DOG>: Arf! Arff! Arf! Arf! Arff! Arf Arf! Arff! Arf Arf!", ConsoleColor.Magenta); Thread.Sleep(500);

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.DOG_HI); Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t   <DOG>: Arf! Arff! Arf! Arf! Arff! Arf Arf! Arff! Arf Arf!", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: Hahahah~ What a cute fella, right? I guess they're also asking for the same thing. A companion.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: Since you're so cute, how about we call you <Athan>? It can mean 'immortal' or 'to hear'.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: Perfect for a good dog like you!!", ConsoleColor.Magenta); Thread.Sleep(500);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.FRIEND_HI); Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t   ATHAN: Awwooooooo! Awoooo!", ConsoleColor.Magenta); Thread.Sleep(500);
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.DOG_HI); Thread.Sleep(1000);

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Typing.AnimateType("\n\n\t\t\t   PAL: *chuckles", ConsoleColor.Magenta); Thread.Sleep(500);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            Typing.AnimateType("\n\t\t\t                            Hahah, don't worry I don't mind your presence at all.", ConsoleColor.DarkYellow); Thread.Sleep(400);
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.HI); Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Typing.AnimateType("\n\n\t\t\tI don't know how the journey will be from now on, but I know it will be fun. The more the merrier, they said.", ConsoleColor.DarkYellow); Thread.Sleep(400);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey, this not, not alone");

            for (int i = 0; i < 5; i++)
            {
                color.TEXT("\n .\n\n", ConsoleColor.Green);
                Thread.Sleep(1000);
            }

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Typing.AnimateType("\n\t\t\t\t      The child, after traveling alone for a long time, finally met 'friends' along the way", ConsoleColor.Green); Thread.Sleep(400);
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\tDays soon turned to Weeks to Months...", ConsoleColor.Green); Thread.Sleep(400);
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t        But then one day...", ConsoleColor.Green); Thread.Sleep(400);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            Console.WriteLine(Animation.FRIEND_SAD); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            Typing.AnimateType("\n\n\t\t\t   PAL: Hey, I'm really happy I found someone like you on this journey.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: It made things easy for me, emotionally and all, haha.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: But this time, I have to part ways with you, I found a new path I want to venture and explore.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t   PAL: I don't know if we'll ever meet again, but I'll cherish every moment we had.", ConsoleColor.Magenta); Thread.Sleep(500);
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t   ATHAN: Awwooooooo! Awoooo!", ConsoleColor.Magenta); Thread.Sleep(500);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.DOG_SAD); Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Typing.AnimateType("\n\t\t\t\t\t\t\t\tIt's hard to see you guys go...", ConsoleColor.DarkYellow); Thread.Sleep(400);
            Typing.AnimateType("\n\n\t\t\t\t\t\t   This journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

            for (int i = 0; i < 8; i++) Console.WriteLine();

            while (true)
            {
                Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\u001b[5m Separation is hard for everyone, especially with those close to you.  \u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5m The [$Y$TEM] wants to give you a chance.   \u001b[0m");
                Console.Write($"\n\n\t\t\t\u001b[5m    Will you continue your journey with either of them or go alone? You MUST only choose one of them.   \u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\u001b[5m 1) Accompany <Pal>   \u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\u001b[5m 2) Accompany <Athan>   \u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\u001b[5m 3) Go Alone   \u001b[0m");
                Console.Write($"\n\n\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                string? choice = Console.ReadLine();
                Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                Thread.Sleep(2000);

                historyLog.QuickLog("SYSTEM", "An option is given");
                if (choice == "1")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go with PAL");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow); Thread.Sleep(400);
                    color.TEXT("\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n\n...So you won't mind if I follow you this time, right <Pal>?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'll be the companion you've been looking for...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\n\t\t\t   PAL: *Gasp!! N-no way!", ConsoleColor.Magenta); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: I don't mind at all. I'm happy to have you by my side, too!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Together, we can explore new places, experience difficulties along the way...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: maybe a fight or two, but we'll be fine as long as we're together...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_HI); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\nHey buddy, I'll miss you very much too. Are you sure you won't follow us?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nTogether we'll be the greatest team that'll ever be...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.HUG); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\nGoodbye <Athan>, I hope you'll meet new friends in your journey too...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(900);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> and PAL continues their journey");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green); historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to use the MAKESHIFT RAFT");
                    historyLog.QuickLog("SYSTEM", "TIMESKIP TO 10 YEARS");
                    historyLog.QuickLog("SYSTEM", "Things aren't exactly the same anymore");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nDays turned to Weeks.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWeeks turned to Months.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nAnd Months turned to Years.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nOh, look, ten years have now passed?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYet the two companions never separated. They were tight as a glue!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey had distinct and unique personalities, but this only brought them closer as they learn from each other.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nFriendship bloomed, tighter than a knot.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey only have each other to rely on. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nHowever, even such a beautiful friendship can never avoid conflicts. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nConflict that hurt like a twisting knife. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nConflict that could forever damage a relationship, even if it's only a simple mistake. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nLike a wise man once said, 'trust takes years to build, seconds to break, and forever to repair'... ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n'<Pal> Please, you can't do this to me! We can still make it right...!' ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nGuilt that swallows the poor child whole ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nRegrets haunted the child every day. They once again felt loneliness they haven't felt for years...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m"); ;
                        Console.Write($"\n\n\t\t\t\t\u001b[5m 1) Cling to them and do whatever it takes to make things right  \u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\u001b[5m 2) Let them go. They are tired. You are tired. You will only drain each other dry.   \u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\t\t\u001b[5m Choice: \u001b[0m");
                        string? choose = Console.ReadLine();
                        Console.Write("\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "An option is given");
                        if (choose == "1")
                        {
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(600);
                            }
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to cling to PAL");
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\n\nOh, what a toxic relationship this has turned to be.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nFriendship that's preserved by locks and chains.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe child only kept clinging!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nHowever, things can never be the same again.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nTheir once-best-ever-friend now looks at them with contempt and distrust", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe child knew this would happen-- ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nBut their friend was the only person they've ever known since coming to this world!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nSo they cling and CLING and eventually, they were pushed away, never to be seen again.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nOh, what a tragic ending this has turned out to be.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000); break;
                        }

                        else if (choose == "2")
                        {
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(600);
                            }
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to let go of PAL");
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\n\nThe child made a big sacrifice.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nIt's never easy to let someone go.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nBut it's something that must be done eventually.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nWhy hold on to a relationship that could only destroy you and kill you slowly, right?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nSeparation is painful, but sometimes it's what's best for everyone.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nIf you love them, you let them go, right?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nChin up, things will be okay.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(2000);
                            break;
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Console.Clear();
                        }
                    }

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    if (ageValue >= 10 && ageValue <= 20)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> slowly continues their journey again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 21 && ageValue <= 40)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT_SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT_SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT, repeat: 1, delay: 1200, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> slowly continues their journey again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 41 && ageValue <= 50)
                    {
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OLD); Thread.Sleep(1000);
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        historyLog.QuickLog("SYSTEM", "'???' switched from Narration Mode to Invisible Character Mode");

                        Typing.AnimateType("\n\nMy child, look how old you are now...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\n\n\nWho...? Hmm but yea... I don't think I can continue any longer.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to stay here, maybe build a house or something.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nThere are a lot of things life can still offer.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to settle here and start life anew.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);

                        for (int i = 0; i < 9; i++) Console.WriteLine();
                        Typing.AnimateType($"\t\t\t\t\t\t\t\t I see. It's great knowing you <{currentPlayer.userName}>", ConsoleColor.Green); Thread.Sleep(1500);

                        saveSystem.SaveCheckpoint("End8", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t\t\t\t 'Retirement is not the end of the road;", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t it is the beginning of the open highway.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Retirement' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        return;
                    }
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else if (choice == "2")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go with ATHAN");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow);
                    color.TEXT("\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n...So I won't say goodbye, my friend.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIf it's our destiny to meet again, we'll surely find each other when the time is right.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm gonna miss you <Pal>! Take care!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.LOW); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\n\t\t\t   PAL: Aww, I will miss you too, my dear friend!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Thank you for letting me be a part of your journey. Until then...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nI guess it's you and I now, <Athan>", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI know you're also going to a separate way but... you wouldn't mind if I accompany you, right?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI never thought I would hate being alone again.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.HUG); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nAwww~ I'll take that as a yes then hehee...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nFrom now on, you'll be my family! A parent, a sibling, or even my child!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Typing.AnimateType("\t\t\t\t\t\t\t\t\t   ATHAN: Awwooooooo! Awoooo!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> and ATHAN continues their journey");

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    historyLog.QuickLog("SYSTEM", "Timeskip to 10 years");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nDays turned to Weeks.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWeeks turned to Months.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nAnd Months turned to Years.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYears passed by so quickly...  I see that it's been 10 years already?", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut despite the language barriers and species differences, neither of them seemed to mind at all.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nNever were they separated. Such loyalty, such love!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe presence of one another helped fill the gap of the trio they once were.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut slowly, despite being so close, you didn't know how <Athan> was feeling.. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nWhat <Athan> was experiencing. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey're getting weaker as days pass by. They hardly eat at all, always sleeping in your lap.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYou know something's happening, but you can't quite understand what's happening. Until..", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\n'<Athan!> Where are you, buddy? Please make any noise so I can find you!'", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n'Please tell me we’re just playing hide and side-- Come on, where are you?'", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\nIt's been hours since you've last seen them. They disappeared without any trace!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe food, water, and toys were all untouched.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n'Were they kidnapped?', you thought for a second.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\nNot until you saw them, peacefully sleeping...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_DIE); Thread.Sleep(1000);
                    for (int i = 0; i < 8; i++) Console.WriteLine();

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> found ATHAN dead");

                    Typing.AnimateType("\n\n'Buddy? You scared me! Were you tire--'", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nNo heartbeat. No gentle breaths you are used to. ", ConsoleColor.Red, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> IS DEAD.", ConsoleColor.Red, "center"); Thread.Sleep(400);

                    Typing.AnimateType("\n\n\n\n\n\nNo, they're just sleeping, I'm sure!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\n\n\n\nNo, they're dead. And you haven't realized that all this time, he was dying.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> left you; they knew they were dying today.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThey knew this would only hurt you.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThe child... You knew this day was coming.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nYou knew something was going on! Yet you didn't do anything!", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("Oh, dear <Athan>... why didn't you tell me?", ConsoleColor.DarkYellow); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI could have stayed by your side and let you feel safe.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI could have showered you with enough warmth until your last breath...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "'???' switched from Narration Mode to Invisible Character Mode");

                    Typing.AnimateType("\n\nMy dear child, please rise, you still need to continue your journey.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n<Athan> has already finished their role, their journey in this world.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nLive for him. Live for his sake.", ConsoleColor.Green, "center"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\n\n\n\nGoodbye <Athan>...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(5000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    if (ageValue >= 10 && ageValue <= 20)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.BURIAL[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.BURIAL, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 15; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY);
                        for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey, this time, alone again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 21 && ageValue <= 40)
                    {
                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.BURIAL, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                        for (int i = 0; i < 10; i++) Console.WriteLine();

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT_SAD[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT_SAD, repeat: 1, delay: 600, yOffset: safeY);
                        for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(1000);

                        safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.ADULT[0].Split('\n').Length);
                        Typing.AnimateFrames(Animation.ADULT, repeat: 1, delay: 1200, yOffset: safeY);
                        for (int i = 0; i < 105; i++) Console.WriteLine(); Thread.Sleep(3000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey, this time, alone again");
                        Thread.Sleep(1000);
                    }

                    else if (ageValue >= 41 && ageValue <= 50)
                    {
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Console.WriteLine(Animation.OLD);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Typing.AnimateType("\n\nMy child, look how old you are now...", ConsoleColor.Green, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\n\n\n Please, can't I stay here together with them?", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI'm already old, I can't handle the long journey ahead.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nPlus, I'm not used to traveling alone anymore...", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nI want to settle here and start life anew. I just want to rest already.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nMaybe make a house or something...", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        Typing.AnimateType("\n\nAnything... just so that can't leave this place behind.", ConsoleColor.Yellow, "center"); Thread.Sleep(400);
                        for (int i = 0; i < 10; i++) Console.WriteLine();
                        Typing.AnimateType($"I see. It's great knowing you <{currentPlayer.userName}>", ConsoleColor.Green, "center"); Thread.Sleep(1500);

                        saveSystem.SaveCheckpoint("End9", "");
                        Continue.ContinueOrExit();
                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Typing.Blink("\n\n\n\n\n\n\t\tVictoria Erickson >>", blinkcnt: 2, on: 500, off: 300);
                        Typing.AnimateType("\n\n\t\t 'Sometimes the end of a journey isn't a place on a map, but the moment your heart tells you it's time to stay.", ConsoleColor.Red, "left");
                        Typing.AnimateType("\n\n\t\t\t\t\t\t\t\tEspecially when love — even the kind with four paws — plants its roots.'", ConsoleColor.Red, "left");
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "<<PLAYER>> decided to end their journey");
                        historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                        historyLog.QuickLog("SYSTEM", "'Mourning' Ending Route Discovered");

                        for (int i = 0; i < 4; i++) Console.WriteLine();
                        Console.WriteLine(Animation.bye);
                        Environment.Exit(0);
                    }
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else if (choice == "3")
                {
                    saveSystem.SaveCheckpoint("Trial2", choice);
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to continue their journey alone");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    color.TEXT("\nIt's hard to see you guys go...", ConsoleColor.DarkYellow);
                    color.TEXT("\nThis journey has been the most memorable for me too...", ConsoleColor.DarkYellow); Thread.Sleep(400);

                    Typing.AnimateType("\n\n...So I won't say goodbye, my friend.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIf it's our destiny to meet again, we'll surely find each other when the time is right.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm gonna miss you <Pal>! Take care!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.LOW); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\n\t\t\t   PAL: Aww, I will miss you too, my dear friend!", ConsoleColor.Magenta, "left"); Thread.Sleep(500);
                    Typing.AnimateType("\n\n\t\t\t   PAL: Thank you for letting me be a part of your journey. Until then...", ConsoleColor.Magenta, "left"); Thread.Sleep(500);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.FRIEND_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DUO); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("Hey buddy... Are you sure you're okay being alone?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_SAD); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nHm... Is that so... I guess I'll take that as a yes. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI don't know where you're going, but I pray for your safe travels.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI hope you'll meet a good friend or two on your journey, too. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.DOG_HI); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Typing.AnimateType("\n\nI will miss you, buddy, I won't say goodbye like I did for <Pal>", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n...I hope our paths interconnect again. ", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nHm...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nWow... I never thought I'd be alone again in this journey.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIt feels so wrong without anyone to talk to... anyone present near me.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nBut I want to respect their decisions, I know that eventually we'll end up separating.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI'm used to this so.. I should be fine...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.SAD[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.SAD, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(3000);

                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    safeY = Math.Min(Console.BufferHeight - 1, Console.CursorTop + Animation.CONTINUE[0].Split('\n').Length);
                    Typing.AnimateFrames(Animation.CONTINUE, repeat: 1, delay: 600, yOffset: safeY); Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++) Console.WriteLine();

                    Thread.Sleep(1000);
                    Checkpoint CHECK = new Checkpoint();
                    CHECK.POINT();
                    break;
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Console.Clear();
                }
            }
        }
    }

    internal class Trial3 : NewGame, IAnimation, IChecks
    {
        ColoredText color = new ColoredText();
        HistoryLog historyLog = new HistoryLog();
        Continue saveSystem = new Continue();

        public void SaveCheckpoint(string choice)
        {
            saveSystem.SaveCheckpoint("Trial3", choice);
        }

        static int TimeoutChoice(int timeout)
        {
            int choice = 0;
            DateTime end = DateTime.Now.AddSeconds(timeout);

            while (DateTime.Now < end)
            {
                if (Console.KeyAvailable)
                {
                    char ch = Console.ReadKey(true).KeyChar;
                    if (ch == '1' || ch == '2' || ch == '3')
                    {
                        choice = ch - '0';
                        break;
                    }
                }

                Console.Write($"\r\t\t\t\t\t\t\t\tYou have {(end - DateTime.Now).Seconds} seconds left!   ");
                Thread.Sleep(100);
            }

            return choice;
        }

        private static bool animationPlayed = false;

        public void ContinueAnimation(string decide) //continue
        {
            Console.Clear();
            Console.WriteLine($"Resuming the Third Trial from Choice {decide}..."); Thread.Sleep(2000);
            Console.Clear();
            int safeY;
            while (true)
            {
                if (decide == "1")
                {
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Continue.ContinueOrExit();
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to use the MAKESHIFT RAFT");

                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Typing.AnimateType("HEHE! Then first stop will be uncharted waters!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.SHORE); Thread.Sleep(1000);
                    for (int i = 0; i < 8; i++) Console.WriteLine();

                    int yStart = Console.CursorTop;
                    for (int frame = 0; frame < Animation.SAIL.Length; frame++)
                    {
                        Console.SetCursorPosition(0, yStart);
                        Console.Write(new string(' ', Console.WindowWidth * 15)); //20
                        Console.SetCursorPosition(0, yStart);
                        Console.Write(Animation.SAIL[frame]);
                        Thread.Sleep(900);
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(500);
                    }
                    for (int i = 0; i < 12; i++) Console.WriteLine();
                    Thread.Sleep(1000);

                    int frameHeight = Animation.WAVE[0].Split('\n').Length;
                    safeY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - frameHeight);

                    if (!animationPlayed)
                    {
                        animationPlayed = true;

                        foreach (var frame in Animation.WAVE)
                        {
                            var lines = frame.Split('\n');

                            for (int i = 0; i < lines.Length && safeY + i < Console.BufferHeight; i++)
                            {
                                Console.SetCursorPosition(0, safeY + i);
                                Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                            }

                            Thread.Sleep(1000);
                        }
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Typing.AnimateType("The waves are passing me!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 9; i++) Console.WriteLine();

                    yStart = Console.CursorTop;
                    Console.WriteLine(Animation.STORM); Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Typing.AnimateType("I'm finally inside the storm... I hope I'll make it through like Odysseus...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 15; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 18; i++) Console.WriteLine();
                    Thread.Sleep(1000);

                    int height = Animation.STORMY[0].Split('\n').Length;
                    int safe = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - height);

                    animationPlayed = true;

                    foreach (var frame in Animation.STORMY)
                    {
                        var lines = frame.Split('\n');

                        for (int i = 0; i < lines.Length && safe + i < Console.BufferHeight; i++)
                        {
                            Console.SetCursorPosition(0, safe + i);
                            Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                        }

                        Thread.Sleep(1000);
                    }


                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> is drowning.");

                    Typing.AnimateType("\nUghhh, someone help me! Anyone!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nPlease!! I was so near! It can't just end like this!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 11; i++) Console.WriteLine();

                    Console.WriteLine(Animation.HOLD_ON); Thread.Sleep(1000);
                    for (int i = 0; i < 23; i++) Console.WriteLine();

                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        color.TEXT("\n\n\u001b[5m CHOOSE QUICKLY  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\t\t\t\u001b[5m  You have to let go of the raft and swim!\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m     You must do what it takes to survive!   \u001b[0m");
                        color.TEXT("\n\n\u001b[5m WARNING: You don't know how to swim  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\u001b[5m 1)Cling to the raft's debris\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 2)Try to swim \u001b[0m");

                        int timeout = 10;

                        color.TEXT("\n\n\u001b[5mCHOOSE! \u001b[0m", ConsoleColor.DarkRed);
                        for (int i = 0; i < 1; i++) Console.WriteLine();
                        int raftChoice = TimeoutChoice(timeout);

                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "An urgent option is given");

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        if (raftChoice == 0)
                        {
                            historyLog.QuickLog("SYSTEM", "Time ran out");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("N-no wait- haha- wait WAIT!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\nMaybe there's another way I could get there with this remaining wood!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\nMay if I could flutter my way out here--", ConsoleColor.DarkYellow, "center"); Thread.Sleep(550);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            int y = Console.CursorTop;
                            if (!animationPlayed)
                            {
                                animationPlayed = true;
                                for (int frame = 0; frame < Animation.TO_DROWN.Length; frame++)
                                {
                                    Console.SetCursorPosition(0, y);
                                    Console.Write(new string(' ', Console.WindowWidth * 15));

                                    Console.SetCursorPosition(0, y);
                                    Console.Write(Animation.LET_GO[frame]);

                                    Thread.Sleep(1000);
                                }
                            }

                            int HEIGHT = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGHT);

                            animationPlayed = true;

                            foreach (var frame in Animation.TO_DROWN)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }


                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);

                            int Start = Console.CursorTop;
                            for (int i = 0; i < 18; i++) Console.WriteLine();

                            int HEI = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\t\tGPT >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'When the wave crashes and pulls you under,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t there is no choice but to surrender to the depths--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t no light, no breath, just a cold embrace of the abyss,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\twhere rising is like a dream forgotten.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            for (int i = 0; i < 3; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }


                        else if (raftChoice == 1)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to cling to the MAKESHIFT RAFT");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("N-no wait- haha- wait WAIT", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nMaybe there's another way I could get there with this remaining wood!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400); Thread.Sleep(400);
                            Typing.AnimateType("\n\nMay if I could flutter my way out here--\n\n\n", ConsoleColor.DarkYellow, "center");
                            for (int i = 0; i < 8; i++) Console.WriteLine();

                            int HEI = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);
                            for (int i = 0; i < 20; i++) Console.WriteLine();

                            int hei = Animation.DROWNS[0].Split('\n').Length;
                            int saf = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - hei);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNS)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && saf + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, saf + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\t\tGPT >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t 'When the wave crashes and pulls you under,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t there is no choice but to surrender to the depths--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t no light, no breath, just a cold embrace of the abyss,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\twhere rising is like a dream forgotten.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            for (int i = 0; i < 3; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Not Let Go' Ending Route Discovered");

                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (raftChoice == 2)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to try swimming");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\nI know I'll still die if I hang on to the raft", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nI'd rather take the risk and see if I survive!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 9; i++) Console.WriteLine();
                            int hei = Animation.LET_GO[0].Split('\n').Length;
                            int saf = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - hei);
                            animationPlayed = true;

                            foreach (var frame in Animation.LET_GO)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && saf + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, saf + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("WAhhhh! I'm so glad I swam away from there!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 10; i++) Console.WriteLine();

                            int heigh = Animation.SWIM[0].Split('\n').Length;
                            int Safe = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - heigh);
                            animationPlayed = true;

                            foreach (var frame in Animation.SWIM)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && Safe + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, Safe + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("Almost there! It's hard to breath but just a little bit more!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 10; i++) Console.WriteLine();

                            animationPlayed = true;
                            foreach (var frame in Animation.SWIM)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && Safe + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, Safe + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 20; i++) Console.WriteLine();
                            Console.WriteLine(Animation.BEACH); Thread.Sleep(1500);
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Clear();
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE1); Thread.Sleep(2000);

                            Console.Clear();
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE2); Thread.Sleep(2000);

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\t\tHaruki Murakami >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'And once the storm is over, you won’t remember how you made it through...", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\tWhen you come out of the storm, you won’t be the same person who walked in.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Environment.Exit(0);
                        }
                    }
                }

                else if (decide == "2")
                {
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to walk on the BRIDGE");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nHEHE! Even if this path is much longer, it's better to be safe than sorry!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(800);
                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

                    Console.WriteLine(Animation.BRIDGE); Thread.Sleep(1000);
                    for (int i = 0; i < 28; i++) Console.WriteLine();

                    int HEIGHT = Animation.WALK[0].Split('\n').Length;
                    int SAFEY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGHT);
                    animationPlayed = true;

                    foreach (var frame in Animation.WALK)
                    {
                        var lines = frame.Split('\n');

                        for (int i = 0; i < lines.Length && SAFEY + i < Console.BufferHeight; i++)
                        {
                            Console.SetCursorPosition(0, SAFEY + i);
                            Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                        }

                        Thread.Sleep(900);
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < 7; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nAhhhhh!! What should I do??", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nI can't just go back now; it's very dangerous, and I should be at the center too!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nThere were also planks that I nearly broke earlier, they might collapse this time if I go back.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nPlus, I can't see what's ahead and behind, they're all covered with fog!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nSwimming is dangerous too, I don't know what's hiding deep in there..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nPlus, I don't know how to swim!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nNow..., jumping all the way through is a suicide..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nIf I stay here, the bridge could give up any time...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\n\n\nThe child raised their head hoping for an answer for me.", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nHowever, this situation is something the child must overcome alone.", ConsoleColor.Green, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        color.TEXT("\n\n\u001b[5m CHOOSE QUICKLY  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\t\t\t\u001b[5m   You could be struck by the lightning next!\u001b[0m");
                        Console.Write("\n\n\n\t\t\t\t\u001b[5m 1) Go back to the start\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 2) Run and jump as high as you can to reach the end\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 3) Try to find a another way... \u001b[0m");
                        int timeout = 10;
                        color.TEXT("\n\n\u001b[5m CHOOSE! \u001b[0m", ConsoleColor.DarkRed);
                        for (int i = 0; i < 1; i++) Console.WriteLine();
                        int bridgeChoice = TimeoutChoice(timeout);

                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "An urgent option is given");

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        if (bridgeChoice == 0)
                        {
                            historyLog.QuickLog("SYSTEM", "Time ran out");
                            Typing.AnimateType("\nWait- whhatt?? I'm still not sure which is the right option! Give me more time?!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.SCARED); Thread.Sleep(1000);
                            for (int i = 0; i < 25; i++) Console.WriteLine();

                            int HEI = Animation.LIMIT[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.LIMIT)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(700);
                            }

                            for (int i = 0; i < 15; i++) Console.WriteLine(); Thread.Sleep(1000);

                            int HEIG = Animation.DROWNING[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIG);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n'In the end, we only regret the chances we didn't take.'", ConsoleColor.Red, "center");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 1)
                        {
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go back");

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("\nEvery option is too risky...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe only way is to return to the beginning and maybe...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nI can find other...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            int HEIG = Animation.GO_BACK[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIG);
                            animationPlayed = true;

                            foreach (var frame in Animation.GO_BACK)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(1000);
                            }
                            for (int i = 0; i < 15; i++) Console.WriteLine();
                            Thread.Sleep(1000);

                            int HEI = Animation.DROWNING[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 15; i++) Console.WriteLine();
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tThomas A. Edison >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'Many of life's failures are people who did not realize", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\thow close they were to success when they gave up.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Can't Go Back' Ending Route Discovered");


                            for (int i = 0; i < 3; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 2)
                        {
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> recklessly plans to jump");
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                            Typing.AnimateType("\n\n\n\nIt wouldn't hurt to try! If I fail, then I fail!!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            int HEI = Animation.MUST[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.MUST)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(700);
                            }
                            for (int i = 0; i < 13; i++) Console.WriteLine();
                            Thread.Sleep(1000);

                            int HEIGH = Animation.DROWNING[0].Split('\n').Length;
                            int SAFY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGH);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFY + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFY + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1500);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n'Some people would rather die trying than live wondering'", ConsoleColor.Red, "center");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Well, I Tried' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 3)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> plans a way to cross the bridgeg safely");

                            Typing.AnimateType("\nHmm... Any other options are impossible for me.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\nI need to think quickly before anything could happen..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            int HEIGH = Animation.SWING[0].Split('\n').Length;
                            int SAFY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGH);
                            animationPlayed = true;

                            foreach (var frame in Animation.SWING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFY + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFY + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(800);
                            }
                            Thread.Sleep(1500);

                            Console.Clear();
                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE1); Thread.Sleep(1500);

                            Console.Clear();
                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE2); Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully makes it to the other side");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");


                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n\t\t\t\t\t\t 'All my steps, all my stumbles--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t they were always bringing me here.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Environment.Exit(0);
                        }
                    }
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
            }
        }

        public void End10() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\t\tGPT >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\t\t\t 'When the wave crashes and pulls you under,", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t there is no choice but to surrender to the depths--", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t no light, no breath, just a cold embrace of the abyss,", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\twhere rising is like a dream forgotten.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 4; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);
            for (int i = 0; i < 3; i++) Console.WriteLine();

            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void End11() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\t\tHaruki Murakami >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\t\t\t 'And once the storm is over, you won’t remember how you made it through...", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\tWhen you come out of the storm, you won’t be the same person who walked in.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void End12() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\n'In the end, we only regret the chances we didn't take.'", ConsoleColor.Red, "center");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);

            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

            for (int i = 0; i < 4; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void End13() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\n\n\t\tThomas A. Edison >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\t\t\t 'Many of life's failures are people who did not realize", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\thow close they were to success when they gave up.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Can't Go Back' Ending Route Discovered");


            for (int i = 0; i < 3; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void End14() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\n'Some people would rather die trying than live wondering'", ConsoleColor.Red, "center");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Well, I Tried' Ending Route Discovered");

            for (int i = 0; i < 4; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }
         
        public void End15() //end
        {
            Console.Clear();
            Console.Write($"\n\tContinuing..."); Thread.Sleep(2000);
            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Typing.Blink("\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
            Typing.AnimateType("\n\n\n\t\t\t\t\t\t 'All my steps, all my stumbles--", ConsoleColor.Red, "left");
            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t they were always bringing me here.'", ConsoleColor.Red, "left");
            for (int i = 0; i < 5; i++) Console.WriteLine();

            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
            Thread.Sleep(2000);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

            for (int i = 0; i < 4; i++) Console.WriteLine();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
        }

        public void PlayAnimation() //newgame
        {
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the 3rd Trial");

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

            historyLog.QuickLog("SYSTEM", "<<PLAYER>> is stuck again.");

            for (int i = 0; i < 5; i++) Console.WriteLine();
            Typing.AnimateType("\nAnother dead-end..?", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Console.WriteLine(Animation.OBSTABLE2); Thread.Sleep(1000);
            for (int i = 0; i < 10; i++) Console.WriteLine();

            Typing.AnimateType("\nThe child looked up and contemplated what to do. They seemed pitiful...", ConsoleColor.Green, "center"); Thread.Sleep(400);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m\n", ConsoleColor.DarkRed);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Animation.GLITCH); Thread.Sleep(700); Console.ResetColor();
            color.TEXT("\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.DarkRed);

            for (int i = 0; i < 10; i++) Console.WriteLine();
            Thread.Sleep(2000);

            historyLog.QuickLog("???", "I want to help them...");
            historyLog.QuickLog("SYSTEM", "'???' switched from Narration Mode to Invisible Character Mode");

            Typing.AnimateType("\nOh my poor child...", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nContinuing the journey ahead will be full of dangers.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);
            for (int i = 0; i < 5; i++) Console.WriteLine();
            Typing.AnimateType("\n\nWHA- WHO'S THERE??", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
            for (int i = 0; i < 4; i++) Console.WriteLine();
            Typing.AnimateType("\n\n\nI am Nobody. However, I've been watching you since you've arrive here. And finally, you're nearing the end.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nIn front of you...", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nIs the cold, raging sea ready to devour you anytime.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nRuthless storms and hungry sea creatures are ahead, as if Poseidon is trying to kill you.", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nLightning bolts so loud that it might be as if Zeus wants to take his revenge.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nNow, what will you do?", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nThe journey is nearing its end, but here you are.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\n\nTell me, what will you choose?", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);

            for (int i = 0; i < 8; i++) Console.WriteLine();
            color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.DarkRed);
            color.TEXT("\n\n\u001b[5mTELL US, WHAT WILL YOU CHOOSE?  \u001b[0m", ConsoleColor.DarkRed);
            color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.DarkRed);
            Thread.Sleep(2000);
            for (int i = 0; i < 9; i++) Console.WriteLine();
            historyLog.QuickLog("SYSTEM", "An option is given");

            Typing.AnimateType("There's a makeshift raft, ready to sail anytime.", ConsoleColor.DarkGreen, "center"); Thread.Sleep(400);
            Typing.AnimateType("\n\nThere's also this old bridge, waiting for someone to walk on it.", ConsoleColor.DarkRed, "center"); Thread.Sleep(400);

            for (int i = 0; i < 9; i++) Console.WriteLine();
            while (true)
            {
                color.TEXT("\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\u001b[5mMakeshift Raft (1)  \u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\u001b[5m > Advantage: Faster and convenient \u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\u001b[5m > Disadvantage: Fragile; could sink anytime  \u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\n\n\u001b[5mBridge (2)  \u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\u001b[5m > Advantage: Won't sink \u001b[0m", ConsoleColor.DarkRed);
                color.TEXT("\n\n\u001b[5m > Disadvantage: Longer journey. Fragile footholds.  \u001b[0m", ConsoleColor.DarkRed); Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\n\n\t\t\t\t\t\t\t\t      \u001b[5m Choice: \u001b[0m");
                string? decide = Console.ReadLine();
                Console.ResetColor();
                color.TEXT("\n\n\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m", ConsoleColor.DarkRed);
                Thread.Sleep(2000);
                for (int i = 0; i < 5; i++) Console.WriteLine();

                if (decide == "1")
                {
                    Console.Write("\x1b[3J");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Continue.ContinueOrExit();
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to use the MAKESHIFT RAFT");

                    for (int i = 0; i < 8; i++) Console.WriteLine();
                    Typing.AnimateType("HEHE! Then first stop will be uncharted waters!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Console.WriteLine(Animation.SHORE); Thread.Sleep(1000);
                    for (int i = 0; i < 8; i++) Console.WriteLine();

                    int yStart = Console.CursorTop;
                    for (int frame = 0; frame < Animation.SAIL.Length; frame++)
                    {
                        Console.SetCursorPosition(0, yStart);
                        Console.Write(new string(' ', Console.WindowWidth * 15)); //20
                        Console.SetCursorPosition(0, yStart);
                        Console.Write(Animation.SAIL[frame]);
                        Thread.Sleep(900);
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(500);
                    }
                    for (int i = 0; i < 12; i++) Console.WriteLine();
                    Thread.Sleep(1000);

                    int frameHeight = Animation.WAVE[0].Split('\n').Length;
                    int safeY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - frameHeight);

                    if (!animationPlayed)
                    {
                        animationPlayed = true;

                        foreach (var frame in Animation.WAVE)
                        {
                            var lines = frame.Split('\n');

                            for (int i = 0; i < lines.Length && safeY + i < Console.BufferHeight; i++)
                            {
                                Console.SetCursorPosition(0, safeY + i);
                                Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                            }

                            Thread.Sleep(1000);
                        }
                    }

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Typing.AnimateType("The waves are passing me!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 9; i++) Console.WriteLine();

                    yStart = Console.CursorTop;
                    Console.WriteLine(Animation.STORM); Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Typing.AnimateType("I'm finally inside the storm... I hope I'll make it through like Odysseus...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    for (int i = 0; i < 5; i++)
                    {
                        color.TEXT("\n .\n\n", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    for (int i = 0; i < 18; i++) Console.WriteLine();
                    Thread.Sleep(1000);

                    int height = Animation.STORMY[0].Split('\n').Length;
                    int safe = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - height);

                    animationPlayed = true;

                    foreach (var frame in Animation.STORMY)
                    {
                        var lines = frame.Split('\n');

                        for (int i = 0; i < lines.Length && safe + i < Console.BufferHeight; i++)
                        {
                            Console.SetCursorPosition(0, safe + i);
                            Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                        }

                        Thread.Sleep(1000);
                    }


                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);
                    for (int i = 0; i < 10; i++) Console.WriteLine();

                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> is drowning.");

                    Typing.AnimateType("\nUghhh, someone help me! Anyone!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nPlease!! I was so near! It can't just end like this!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 11; i++) Console.WriteLine();

                    Console.WriteLine(Animation.HOLD_ON); Thread.Sleep(1000);
                    for (int i = 0; i < 23; i++) Console.WriteLine();

                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        color.TEXT("\n\n\u001b[5m CHOOSE QUICKLY  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\t\t\t\u001b[5m  You have to let go of the raft and swim!\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\t\t\t\u001b[5m     You must do what it takes to survive!   \u001b[0m");
                        color.TEXT("\n\n\u001b[5m WARNING: You don't know how to swim  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\u001b[5m 1)Cling to the raft's debris\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 2)Try to swim \u001b[0m");

                        int timeout = 10;

                        color.TEXT("\n\n\u001b[5mCHOOSE! \u001b[0m", ConsoleColor.DarkRed);
                        for (int i = 0; i < 1; i++) Console.WriteLine();
                        int raftChoice = TimeoutChoice(timeout);

                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Thread.Sleep(2000);

                        historyLog.QuickLog("SYSTEM", "An urgent option is given");

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        if (raftChoice == 0)
                        {
                            historyLog.QuickLog("SYSTEM", "Time ran out");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("N-no wait- haha- wait WAIT!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\n\nMaybe there's another way I could get there with this remaining wood!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\n\nMay if I could flutter my way out here--", ConsoleColor.DarkYellow, "center"); Thread.Sleep(550);

                            for (int i = 0; i < 15; i++) Console.WriteLine();
                            int y = Console.CursorTop;
                            if (!animationPlayed)
                            {
                                animationPlayed = true;
                                for (int frame = 0; frame < Animation.TO_DROWN.Length; frame++)
                                {
                                    Console.SetCursorPosition(0, y);
                                    Console.Write(new string(' ', Console.WindowWidth * 15));

                                    Console.SetCursorPosition(0, y);
                                    Console.Write(Animation.LET_GO[frame]);

                                    Thread.Sleep(1000);
                                }
                            }

                            int HEIGHT = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGHT);

                            animationPlayed = true;
                            foreach (var frame in Animation.TO_DROWN)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }


                            for (int i = 0; i < 10; i++) Console.WriteLine();

                            Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);

                            int Start = Console.CursorTop;
                            for (int i = 0; i < 20; i++) Console.WriteLine();

                            int HEI = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            for (int i = 0; i < 10; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\t\tGPT >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'When the wave crashes and pulls you under,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\t\t\t\t\t there is no choice but to surrender to the depths--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t no light, no breath, just a cold embrace of the abyss,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t\t\twhere rising is like a dream forgotten.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 4; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            for (int i = 0; i < 3; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }


                        else if (raftChoice == 1)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to cling to the MAKESHIFT RAFT");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("N-no wait- haha- wait WAIT", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nMaybe there's another way I could get there with this remaining wood!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400); Thread.Sleep(400);
                            Typing.AnimateType("\n\nMay if I could flutter my way out here--\n\n\n", ConsoleColor.DarkYellow, "center");
                            for (int i = 0; i < 8; i++) Console.WriteLine();

                            int HEI = Animation.TO_DROWN[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            Console.WriteLine(Animation.CRASH); Thread.Sleep(1000);
                            for (int i = 0; i < 20; i++) Console.WriteLine();

                            int hei = Animation.DROWNS[0].Split('\n').Length;
                            int saf = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - hei);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNS)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && saf + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, saf + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\t\tGPT >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t 'When the wave crashes and pulls you under,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t there is no choice but to surrender to the depths--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t no light, no breath, just a cold embrace of the abyss,", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\t\t\t\twhere rising is like a dream forgotten.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            for (int i = 0; i < 3; i++) Console.WriteLine();

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Not Let Go' Ending Route Discovered");

                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (raftChoice == 2)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to try swimming");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Typing.AnimateType("\nI know I'll still die if I hang on to the raft", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nI'd rather take the risk and see if I survive!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 9; i++) Console.WriteLine();
                            int hei = Animation.LET_GO[0].Split('\n').Length;
                            int saf = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - hei);
                            animationPlayed = true;

                            foreach (var frame in Animation.LET_GO)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && saf + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, saf + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("WAhhhh! I'm so glad I swam away from there!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 10; i++) Console.WriteLine();

                            int heigh = Animation.SWIM[0].Split('\n').Length;
                            int Safe = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - heigh);
                            animationPlayed = true;

                            foreach (var frame in Animation.SWIM)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && Safe + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, Safe + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("Almost there! It's hard to breath but just a little bit more!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            for (int i = 0; i < 10; i++) Console.WriteLine();

                            animationPlayed = true;
                            foreach (var frame in Animation.SWIM)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && Safe + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, Safe + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 20; i++) Console.WriteLine();
                            Console.WriteLine(Animation.BEACH); Thread.Sleep(1500);
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Clear();
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE1); Thread.Sleep(2000);

                            Console.Clear();
                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE2); Thread.Sleep(2000);

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\t\tHaruki Murakami >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'And once the storm is over, you won’t remember how you made it through...", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\tWhen you come out of the storm, you won’t be the same person who walked in.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Environment.Exit(0);
                        }
                    }
                }

                else if (decide == "2")
                {
                    Continue.ContinueOrExit();
                    Console.Write("\x1b[3J");
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to walk on the BRIDGE");
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nHEHE! Even if this path is much longer, it's better to be safe than sorry!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(800);
                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");

                    Console.WriteLine(Animation.BRIDGE); Thread.Sleep(1000);
                    for (int i = 0; i < 28; i++) Console.WriteLine();

                    int HEIGHT = Animation.WALK[0].Split('\n').Length;
                    int SAFEY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGHT);
                    animationPlayed = true;

                    foreach (var frame in Animation.WALK)
                    {
                        var lines = frame.Split('\n');

                        for (int i = 0; i < lines.Length && SAFEY + i < Console.BufferHeight; i++)
                        {
                            Console.SetCursorPosition(0, SAFEY + i);
                            Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                        }

                        Thread.Sleep(900);
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < 7; i++) Console.WriteLine();
                    Typing.AnimateType("\n\nAhhhhh!! What should I do??", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nI can't just go back now; it's very dangerous, and I should be at the center too!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nThere were also planks that I nearly broke earlier, they might collapse this time if I go back.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nPlus, I can't see what's ahead and behind, they're all covered with fog!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nSwimming is dangerous too, I don't know what's hiding deep in there..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nPlus, I don't know how to swim!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nNow..., jumping all the way through is a suicide..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\nIf I stay here, the bridge could give up any time...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                    for (int i = 0; i < 5; i++) Console.WriteLine();
                    Typing.AnimateType("\n\n\n\nThe child raised their head hoping for an answer for me. ", ConsoleColor.Green, "center"); Thread.Sleep(400);
                    Typing.AnimateType("\n\n\nHowever, this situation is something the child must overcome alone.", ConsoleColor.Green, "center"); Thread.Sleep(400);

                    for (int i = 0; i < 10; i++) Console.WriteLine();
                    while (true)
                    {
                        Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                        color.TEXT("\n\n\u001b[5mCHOOSE QUICKLY  \u001b[0m", ConsoleColor.DarkRed);
                        Console.Write("\n\n\n\t\t\t\t\t\t\t\u001b[5m   You could be struck by the lightning next!\u001b[0m");
                        Console.Write("\n\n\n\t\t\t\t\u001b[5m 1) Go back to the start\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 2) Run and jump as high as you can to reach the end\u001b[0m");
                        Console.Write("\n\n\t\t\t\t\u001b[5m 3) Try to find a another way... \u001b[0m");
                        int timeout = 10;
                        color.TEXT("\n\n\u001b[5m CHOOSE! \u001b[0m", ConsoleColor.DarkRed);
                        for (int i = 0; i < 1; i++) Console.WriteLine();
                        int bridgeChoice = TimeoutChoice(timeout);

                        Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                        for (int i = 0; i < 5; i++) Console.WriteLine();
                        Thread.Sleep(2000);
                        historyLog.QuickLog("SYSTEM", "An urgent option is given");

                        Console.Write("\x1b[3J");
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                        for (int i = 0; i < 5; i++) Console.WriteLine();

                        if (bridgeChoice == 0)
                        {
                            historyLog.QuickLog("SYSTEM", "Time ran out");
                            Typing.AnimateType("\nWait- whhatt?? I'm still not sure which is the right option! Give me more time?!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            Console.WriteLine(Animation.SCARED); Thread.Sleep(1000);
                            for (int i = 0; i < 25; i++) Console.WriteLine();

                            int HEI = Animation.LIMIT[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.LIMIT)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(700);
                            }

                            for (int i = 0; i < 15; i++) Console.WriteLine(); Thread.Sleep(1000);

                            int HEIG = Animation.DROWNING[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIG);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 5; i++) Console.WriteLine(); Thread.Sleep(1000);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n'In the end, we only regret the chances we didn't take.'", ConsoleColor.Red, "center");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Persistent' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 1)
                        {
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> chose to go back");

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Typing.AnimateType("\nEvery option is too risky...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nThe only way is to return to the beginning and maybe...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\nI can find other...", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            int HEIG = Animation.GO_BACK[0].Split('\n').Length;
                            int SAF = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIG);
                            animationPlayed = true;

                            foreach (var frame in Animation.GO_BACK)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAF + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAF + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(1000);
                            }
                            for (int i = 0; i < 15; i++) Console.WriteLine();
                            Thread.Sleep(1000);

                            int HEI = Animation.DROWNING[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 15; i++) Console.WriteLine();
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tThomas A. Edison >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\t\t\t 'Many of life's failures are people who did not realize", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\n\t\t\t\t\t\t\t\t\thow close they were to success when they gave up.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Can't Go Back' Ending Route Discovered");


                            for (int i = 0; i < 3; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 2)
                        {
                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> recklessly plans to jump");
                            color.TEXT("╒≪─┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉──┉┈◈◉◈┈┉─┉┈◈◉◈┈┉-≫╕\n\n", ConsoleColor.Green);
                            Typing.AnimateType("\n\n\n\nIt wouldn't hurt to try! If I fail, then I fail!!", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            int HEI = Animation.MUST[0].Split('\n').Length;
                            int SAFE = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEI);
                            animationPlayed = true;

                            foreach (var frame in Animation.MUST)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFE + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFE + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);

                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            for (int i = 0; i < 5; i++)
                            {
                                color.TEXT("\n .\n\n", ConsoleColor.Green);
                                Thread.Sleep(700);
                            }
                            for (int i = 0; i < 13; i++) Console.WriteLine();
                            Thread.Sleep(1000);

                            int HEIGH = Animation.DROWNING[0].Split('\n').Length;
                            int SAFY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGH);
                            animationPlayed = true;

                            foreach (var frame in Animation.DROWNING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFY + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFY + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1500);

                            for (int i = 0; i < 5; i++) Console.WriteLine();
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> falls from the bridge");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> dies from drowning");

                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n'Some people would rather die trying than live wondering'", ConsoleColor.Red, "center");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Well, I Tried' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else if (bridgeChoice == 3)
                        {
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> plans a way to cross the bridgeg safely");

                            Typing.AnimateType("\nHmm... Any other options are impossible for me.", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);
                            Typing.AnimateType("\n\n\nI need to think quickly before anything could happen..", ConsoleColor.DarkYellow, "center"); Thread.Sleep(400);

                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            int HEIGH = Animation.SWING[0].Split('\n').Length;
                            int SAFY = Math.Clamp(Console.CursorTop, 0, Console.BufferHeight - HEIGH);
                            animationPlayed = true;

                            foreach (var frame in Animation.SWING)
                            {
                                var lines = frame.Split('\n');

                                for (int i = 0; i < lines.Length && SAFY + i < Console.BufferHeight; i++)
                                {
                                    Console.SetCursorPosition(0, SAFY + i);
                                    Console.WriteLine(lines[i].PadRight(Console.WindowWidth));
                                }

                                Thread.Sleep(800);
                            }
                            Thread.Sleep(1500);

                            Console.Clear();
                            for (int i = 0; i < 8; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE1); Thread.Sleep(1500);

                            Console.Clear();
                            for (int i = 0; i < 10; i++) Console.WriteLine();
                            Console.WriteLine(Animation.ARRIVE2); Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> successfully makes it to the other side");
                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> continues their journey");


                            Console.Write("\x1b[3J");
                            Console.Clear();
                            Console.SetCursorPosition(0, 0);
                            Typing.Blink("\n\n\n\n\t\tUnknown >>", blinkcnt: 2, on: 500, off: 300);
                            Typing.AnimateType("\n\n\n\t\t\t\t\t\t 'All my steps, all my stumbles--", ConsoleColor.Red, "left");
                            Typing.AnimateType("\n\t\t\t\t\t\t\t\t\t they were always bringing me here.'", ConsoleColor.Red, "left");
                            for (int i = 0; i < 5; i++) Console.WriteLine();

                            Console.Write("\t\t\t\u001b[5m ╔═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╗\u001b[0m");
                            Console.Write($"\n\n\t\t\t\t\t\t\t\u001b[5mTHANK YOU FOR STICKING WITH US THIS LONG!  \u001b[0m");
                            Console.Write("\n\n\t\t\t\u001b[5m ╚═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧≪ °❈° ≫✧═════✧═════✧═════✧═════✧═════✧═════✧═════✧═════╝\u001b[0m");
                            Thread.Sleep(2000);

                            historyLog.QuickLog("SYSTEM", "<<PLAYER>> reaches the land");
                            historyLog.QuickLog("SYSTEM", "Ending Quote displayed");
                            historyLog.QuickLog("SYSTEM", "'Reaching The Destination' Ending Route Discovered");

                            for (int i = 0; i < 4; i++) Console.WriteLine();
                            Console.WriteLine(Animation.bye);
                            Environment.Exit(0);
                        }

                        else
                        {
                            Console.Clear();
                            historyLog.QuickLog("SYSTEM", "ERROR");
                            color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            Environment.Exit(0);
                        }
                    }
                }

                else
                {
                    Console.Clear();
                    historyLog.QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\nINVALID CHOICE.\n\n\n", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
            }
        }
    }

    internal class Continue
    {
        ColoredText color = new ColoredText();
        private readonly HistoryLog historyLog = new HistoryLog();
        private readonly string saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "game");

        private static string currentUser = "Guest";

        public static void SetCurrentUser(string username)
        {
            currentUser = username;
        }

        public static string GetCurrentUser()
        {
            return currentUser;
        }

        private string GetProgressPath()
        {
            return Path.Combine(saveDirectory, $"{currentUser}_progress.txt");
        }

        public static void AskForUsername()
        {
            Console.Clear();
            Console.Write("\nEnter Nickname: ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                currentUser = "Guest";
            }
            else
            {
                currentUser = input;
            }
            Console.Clear();
        }

        public void SaveCheckpoint(string scene, string choice)
        {
            try
            {
                Directory.CreateDirectory(saveDirectory);

                string progressPath = GetProgressPath();
                string checkpoint = $"{scene}_{choice}";

                File.WriteAllText(progressPath, checkpoint);

                historyLog.QuickLog("SYSTEM", $"Progress saved for {currentUser} at checkpoint: {checkpoint}");

                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n\tSaved progress for {currentUser}: {checkpoint}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                historyLog.QuickLog("ERROR", $"Failed to save progress: {ex.Message}");
                color.TEXT("\nError saving progress. Please try again.\n", ConsoleColor.Red);
            }
        }

        public bool HasSavedProgress()
        {
            string progressPath = GetProgressPath();
            return File.Exists(progressPath) && new FileInfo(progressPath).Length > 0;
        }

        public void LoadAndResume()
        {
            string progressPath = GetProgressPath();

            if (!HasSavedProgress())
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("\n\n\n\nNo saved progress found. Please start a New Game first.\n\n", ConsoleColor.Red);
                historyLog.QuickLog("SYSTEM", $"Attempted to continue without saved progress for {currentUser}.");
                return;
            }

            string checkpoint = File.ReadAllText(progressPath).Trim();
            historyLog.QuickLog("SYSTEM", $"Loaded checkpoint: {checkpoint}");

            if (string.IsNullOrEmpty(checkpoint))
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT("\n\n\n\nCorrupted save file detected. Start a new game.\n\n", ConsoleColor.Red);
                historyLog.QuickLog("SYSTEM", "Corrupted or empty progress file.");
                File.Delete(progressPath);
                return;
            }

            string[] parts = checkpoint.Split('_');
            historyLog.QuickLog("SYSTEM", $"Loaded checkpoint: {checkpoint}");
            if (parts.Length != 2)
            {
                Console.Write("\x1b[3J");
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                color.TEXT($"\n\nCorrupted save file for {currentUser}. Start a new game.\n\n", ConsoleColor.Red);
                historyLog.QuickLog("SYSTEM", $"Invalid checkpoint format for {currentUser}.");
                File.Delete(progressPath);
                return;
            }

            string scene = parts[0];
            string choice = parts[1];

            Trial1 trial1 = new Trial1();
            playerDetails player = new playerDetails();
            Checkpoint CHECK = new Checkpoint();
            Trial2 trial2 = new Trial2(player);
            Trial3 trial3 = new Trial3();
            NewGame start = new NewGame();

            Console.Write("\x1b[3J");
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            color.TEXT($"\n\nResuming progress for {currentUser} from: {scene} (Choice {choice})...\n\n", ConsoleColor.Green);
            Thread.Sleep(1500);

            switch (scene)
            {
                case "End1": start.End1(); break;
                case "Trial1": trial1.ContinueAnimation(choice); break;
                case "TeaTime": trial1.ContinueTea(choice); break;
                case "WallChoice": trial1.ContinueWall(choice); break;
                case "DoorChoice": trial1.ContinueAgain(choice); break;
                case "End2": trial1.DoorOne.End2(); break;
                case "End3": trial1.DoorTwo.End3(); break;
                case "End4": trial1.DoorThree.End4(); break;
                case "End5": trial1.DoorFour.End5(); break;
                case "End6": trial1.DoorFive.End6(); break;
                case "End7": trial1.DoorSix.End7(); break;
                case "Trial2": trial2.ContinueAnimation(choice); break;
                case "Trial3": trial3.ContinueAnimation(choice); break;
                case "End10": trial3.End10(); break;
                case "End11": trial3.End11(); break;
                case "End12": trial3.End12(); break;
                case "End13": trial3.End13(); break;
                case "End14": trial3.End14(); break;
                case "End15": trial3.End15(); break;
                case "End8": trial2.End8(); break;
                case "End9": trial2.End9(); break;
                case "CheckpointChoice": CHECK.ContinuePOINT(choice); break;

                default:
                    color.TEXT("\nUnknown save data. Starting new game...\n", ConsoleColor.Red);
                    File.Delete(progressPath);
                    break;
            }
        }

        public static void ContinueOrExit()
        {
            Continue continueInstance = new Continue();
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\tPress 1 to continue or 2 to exit...        ");
                Console.ResetColor();

                string? choice = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(choice) || choice == "1")
                {
                    Console.Clear();
                    break;
                }
                else if (choice == "2")
                {
                    Exit exit = new Exit();
                    exit.ExitTheGame();
                }
                else
                {
                    Console.WriteLine("\tInvalid option. Try again.");
                    continue;
                }
            }
        }
    }

    internal class About
    {
        HistoryLog historyLog = new HistoryLog();

        public void GameInfo()
        {
            Console.Clear();
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> opened 'About' Menu");
            ColoredText color = new ColoredText();

            Console.WriteLine(Animation.title);
            color.TEXT("\nINTRODUCTION\n\n", ConsoleColor.Green);
            color.TEXT("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━✦❘༻༺❘✦━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓\n", ConsoleColor.Green);
            Console.WriteLine("    " +
                "\t\t\t\t  As the world keeps changing, work pressures and distractions that never seem to end are\n" +
                "\t\t\t\t  overwhelming many people. This has caused many people's emotional and mental health to\n" +
                "\t\t\t\t    decline. Some, fortunately, can seek professional help and improve their situations.\n" +
                "\t\t\t\t  However, many still prefer to keep their conditions private and cope in various ways,\n" +
                "\t\t\t\t                                     including gaming.\r\n\n" +
                "\n\t\t\t\t  Gaming, contrary to common belief, is not entirely harmful; in fact, it can actually be\n" +
                "\t\t\t\t    beneficial. As electronic games continue to advance, mental health professionals are\n" +
                "\t\t\t\t  increasingly using them as a therapeutic tool, finding new methods to engage with their\n" +
                "\t\t\t\t    patients (Granic et al., 2013). These games, often referred to as therapeutic games,\n" +
                "\t\t\t\t     aim to address emotional and psychological needs, promote well-being, and provide\n" +
                "\t\t\t\t                           positive experiences (Reid, 2024).\r\n\r\n\r\n" +
                "\t\t\t\t  ‘Finding Nimo’, which means ‘Finding Yourself,’ is an adventure game that uses ASCII art\n" +
                "\t\t\t\t     and basic C# animation. It is designed to help individuals recognize what might be\n" +
                "\t\t\t\t     missing in their lives and guide them to explore, heal, and grow. It is a type of\n" +
                "\t\t\t\t  therapeutic game where every decision made by the player influences the flow and outcome\n" +
                "\t\t\t\t  of their in-game journey– much like in real life. Every route reveals different endings,\n" +
                "\t\t\t\t  every action has an implication, and every choice makes a difference. ‘Finding Nimo’ aims\n" +
                "\t\t\t\t   to help players reflect on themselves and discover who they really are, as many remain\n" +
                "\t\t\t\t    unaware of their hidden pain and struggles. Through this game, players might not only\n" +
                "\t\t\t\t           find peace and satisfaction but also experience healing from within.\r\n");
            color.TEXT("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━✦❘༻༺❘✦━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛", ConsoleColor.Green);

            historyLog.QuickLog("???", "<<PLAYER>> seems satistfied with the information :)");
        }
    }

    internal class HistoryLog
    {
        private readonly string logPath = @"C:\Users\Tiffany Mae\Documents\game\log.txt";
        private readonly ColoredText color = new ColoredText();
        public void SaveProgress(string checkpointName)
        {
            File.WriteAllText(logPath, checkpointName);
            QuickLog("SYSTEM", $"Progress saved at checkpoint: {checkpointName}");
        }

        public string LoadProgress()
        {
            if (File.Exists(logPath))
            {
                string checkpoint = File.ReadAllText(logPath);
                QuickLog("SYSTEM", $"Progress loaded from checkpoint: {checkpoint}");
                return checkpoint;
            }
            QuickLog("SYSTEM", "No progress file found");
            return "";
        }

        public void QuickLog(string user, string action)
        {
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            using (StreamWriter sw = new StreamWriter(logPath, append: true))
            {
                sw.WriteLine($"[{date} | {time}]");
                sw.WriteLine($"   <{user}>\t\t-\t\t{action}");
                sw.WriteLine("\n--------------------------------------\n");
            }
        }

        public void ShowLog()
        {
            try
            {
                QuickLog("SYSTEM", "<<PLAYER>> Opened 'History' Menu");
                Console.Clear();
                Console.WriteLine(Animation.title);
                Console.Write("\n\nOpen Log File? (y/n): ");
                string? choice = Console.ReadLine()?.ToLower();

                if (!File.Exists(logPath))
                {
                    color.TEXT("FILE DOES NOT EXIST YET!", ConsoleColor.Red);
                    QuickLog("SYSTEM", "ERROR");
                    return;
                }

                if (choice == "y")
                {
                    QuickLog("SYSTEM", "<<PLAYER>> opened History Log File");

                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = logPath,
                        UseShellExecute = true
                    });
                }

                else if (choice == "n")
                {
                    QuickLog("SYSTEM", "<<PLAYER>> declined to open file");

                    Console.Write("\n\nOpen Log Here Instead? (y/n): "); // y/n
                    string? yN = Console.ReadLine()?.ToLower();

                    if (yN == "y")
                    {
                        Console.Clear();
                        QuickLog("SYSTEM", "<<PLAYER>> viewed History Log in Terminal");

                        color.BOTH("\n\n\n\nHISTORY LOG\nas of " + DateTime.Now.ToString("MM/dd/yyyy") + "\n\n\n", ConsoleColor.Black, ConsoleColor.DarkRed);

                        foreach (var line in File.ReadLines(logPath))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(line);
                            Console.ResetColor();
                        }
                        Thread.Sleep(1000);
                    }

                    else if (yN == "n")
                    {
                        Console.Clear();
                        QuickLog("SYSTEM", "<<PLAYER>> decided not to view the  History Log");
                        Console.WriteLine(Animation.history);
                    }

                    else
                    {
                        Console.Clear();
                        QuickLog("SYSTEM", "ERROR");
                        color.TEXT("\n\nINVALID OPTION. TRY AGAIN.\n\n\n", ConsoleColor.Red);
                    }
                }

                else
                {
                    Console.Clear();
                    QuickLog("SYSTEM", "ERROR");
                    color.TEXT("\n\nINVALID OPTION. TRY AGAIN.\n\n\n", ConsoleColor.Red);
                }

                Console.Write("\n\nDelete History Data?(y/n): ");
                string? delete = Console.ReadLine()?.ToLower();

                if (delete == "y")
                {
                    File.Delete(logPath);
                    Console.Clear();
                    color.BOTH("\n\n\n\t\t\t\t\t\t\t\tHistory Log deleted successfully.", ConsoleColor.Black, ConsoleColor.Green);
                    QuickLog("SYSTEM", "Previous History Log data deleted successfully");
                }

                else
                {
                    Console.Clear();
                    color.TEXT("\n\n\nNo history log file found to delete.", ConsoleColor.Red);
                    QuickLog("SYSTEM", "History Log data failed to remove");
                }

                return;
            }

            catch (Exception ex)
            {
                Console.Clear();
                color.TEXT("\n\nNO FILE WAS FOUND\n\n", ConsoleColor.Red);
                Console.WriteLine($"\t\t\t\t\t\t\t\tERROR: {ex.Message}");
                QuickLog("SYSTEM", "ERROR");
            }
        }
    }

    internal class Exit : IAnimation
    {
        HistoryLog historyLog = new HistoryLog();

        public void ExitTheGame()
        {
            Console.Clear();
            Console.WriteLine(Animation.bye);
            Environment.Exit(0);
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> exited the game succesfully.");
        }

        public void PlayAnimation()
        {
            ExitTheGame();
        }
    }

    class Menu
    {
        private int SelectedIndex;
        private string[] Choices; //options
        private string Prompt;
        HistoryLog historyLog = new HistoryLog();

        public Menu(string prompt, string[] choices)
        {
            Prompt = prompt;
            Choices = choices;
            SelectedIndex = 0;
        }

        private void DisplayInMenu()
        {
            Console.Clear();

            Console.WriteLine(Animation.title);
            historyLog.QuickLog("SYSTEM", "<<PLAYER>> Opened Menu");

            Console.WriteLine(Prompt);
            Console.WriteLine();

            for (int i = 0; i < Choices.Length; i++)
            {
                string currentOption = Choices[i];
                string button;

                if (i == SelectedIndex)
                {
                    button = ">> ";
                    ForegroundColor = ConsoleColor.Black;
                    BackgroundColor = ConsoleColor.Green;
                }

                else
                {
                    button = " ";
                    ForegroundColor = ConsoleColor.White;
                    BackgroundColor = ConsoleColor.Black;
                }

                WriteLine($"\n    {button}  {currentOption}");
            }
            Console.ResetColor();
        }

        public int Run()
        {
            ConsoleKey keyPressed;

            do
            {
                Console.Clear();
                DisplayInMenu();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    SelectedIndex--;

                    if (SelectedIndex == -1)
                    {
                        SelectedIndex = Choices.Length - 1;
                    }
                }

                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    SelectedIndex++;

                    if (SelectedIndex == Choices.Length)
                    {
                        SelectedIndex = 0;
                    }
                }
            }

            while (keyPressed != ConsoleKey.Enter);

            return SelectedIndex;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            HistoryLog historyLog = new HistoryLog();

            try
            {
                ColoredText color = new ColoredText();

                for (int i = 0; i < 17; i++) Console.WriteLine();
                color.BOTH("\t\t\t\t\t\t\t\t\tFINDING 'NIMO'\n\n\n", ConsoleColor.Black, ConsoleColor.Green);
                color.TEXT("'Switch to Full Screen for better Gaming Experience'", ConsoleColor.Green);
                Console.Write("\n\n\n\t\t\t\t\t\t\t\t\t\x1b[5m[Press Enter]\x1b[0m");

                historyLog.QuickLog("SYSTEM", "<<PLAYER>> opened the game.");

                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.Clear();

                for (int i = 0; i < 19; i++) Console.WriteLine();
                color.TEXT("\n\t    \u001b[5mLoading...\u001b[0m", ConsoleColor.Green);
                Thread.Sleep(5000);
                Console.Clear();

                for (int i = 0; i < 5; i++) Console.WriteLine();
                Intro intro = new Intro();
                intro.START();
            }

            catch (Exception ex)
            {
                Console.Clear();
                historyLog.QuickLog("SYSTEM", "ERROR");
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
