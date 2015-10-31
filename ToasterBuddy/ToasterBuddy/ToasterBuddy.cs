﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.Networking;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace ToasterBuddy
{
    public static class ToasterBuddy
    {
        private static short _firstHeader = -1;
        private static int _toasterHeader = -1;
        private static GamePacket _toasterGamePacket;
        private static string _toasterText = "Waiting for packet.";
        private const int TimeLimit = 250;
        private static float _startTime = 0;
        private static bool _toasterGamePacketSent = false;
        private static Text text = new Text(_toasterText, new Font("Arial", 20, FontStyle.Bold))
        {
            Color = Color.White,
            Position = new Vector2(10, 10)
        };

        private static bool _toasterGamePacketIsReady
        {
            get { return _toasterGamePacket != null; }

        }

        static void Main(string[] args)
        {
            _startTime = Game.Time;
            Game.OnSendPacket += Game_OnSendPacket;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnTick += Game_OnTick;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_toasterGamePacketIsReady)
            {
                if (TimeLimit <= (Game.Time - _startTime) && !_toasterGamePacketSent)
                {
                    _toasterGamePacket.Send();
                    _toasterGamePacketSent = true;
                }
                if (_toasterGamePacketSent)
                {
                    _toasterText = "Packet sent, the game will start :) Have fun.";
                }
                else
                {
                    _toasterText = "Packet found, you can start the game when you want (using escape or space).\n Time Left: " + Math.Truncate(TimeLimit - (Game.Time - _startTime)) + "s";
                }
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            text.TextValue = _toasterText;
            text.Draw();
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowMessages.KeyDown)
            {
                var escapeKeys = new List<uint> { 27, 32 };
                if (escapeKeys.Contains(args.WParam))
                {
                    _toasterGamePacket.Send();
                    _toasterGamePacketSent = true;
                }
            }
        }

        private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (_firstHeader == -1)
            {
                _firstHeader = args.GamePacket.Header.OpCode;
                return;
            }
            if (_firstHeader != args.GamePacket.Header.OpCode && _toasterHeader == -1)
            {
                _toasterGamePacket = args.GamePacket;
                _toasterHeader = args.GamePacket.Header.OpCode;
                args.Process = false;
                return;
            }
        }

        private static void Print(this GamePacket p)
        {
            Console.WriteLine("Header: " + p.Header.OpCode);
            Console.WriteLine("vTable: " + p.Header.Algorithm.Address);
            for (var i = 0; i < p.Data.Length; i++)
            {
                Console.Write("[" + i + "]" + " = " + p.Data[i] + ", ");
            }
            Console.WriteLine();
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Game.OnTick -= Game_OnTick;
            Game.OnSendPacket -= Game_OnSendPacket;
            Game.OnWndProc -= Game_OnWndProc;
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

    }
}