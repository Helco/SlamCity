namespace DigiChrome.Player;
using System;
using System.Collections.Generic;
using System.IO;
using SDL2;
using static SDL2.SDL;

public static unsafe class Player
{
    private static void CheckSDL(int ret)
    {
        if (ret < 0)
            throw new Exception(SDL_GetError());
    }

    public static void Main(string[] args)
    {
        CheckSDL(SDL_Init(SDL_INIT_AUDIO | SDL_INIT_VIDEO | SDL_INIT_EVENTS));
        var window = SDL_CreateWindow("DigiChrome player", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 30, 30, default);
        var renderer = SDL_CreateRenderer(window, -1, default);
        var isRunning = true;

        using var fileStream = new FileStream(@"C:\dev\SlamCityG\CD1\SLAM\open.avc", FileMode.Open, FileAccess.Read);
        using var decoder = new Decoder(fileStream);
        IntPtr texture = IntPtr.Zero;
        int texW = 30, texH = 30;
        int scale = 1;
        bool isPaused = false;

        var audioFormat = new SDL_AudioSpec()
        {
            format = AUDIO_U8,
            freq = Decoder.AudioFrequency,
            channels = 1,
            samples = 8192
        };
        var audioDeviceId = SDL_OpenAudioDevice(IntPtr.Zero, iscapture: 0, ref audioFormat, out var obtained, allowed_changes: 0);
        if (audioDeviceId == 0)
            throw new Exception(SDL_GetError());
        SDL_PauseAudioDevice(audioDeviceId, pause_on: 0);

        bool hasFrame = false;
        int iii = 0;
        bool EnsureNextFrame()
        {
            if (hasFrame)
                return hasFrame;
            hasFrame = decoder.MoveNext();
            if (!hasFrame)
            {
                decoder.Reset();
                hasFrame = decoder.MoveNext();
            }
            if (hasFrame)
            {
                fixed (void* audioPtr = decoder.Current.Audio)
                    CheckSDL(SDL_QueueAudio(audioDeviceId, new IntPtr(audioPtr), (uint)decoder.Current.Audio.Length));
            }
            return hasFrame;
        }

        while (isRunning)
        {
            while (SDL_PollEvent(out var ev) > 0)
            {
                if (ev.type == SDL_EventType.SDL_QUIT)
                    isRunning = false;
                else if (ev.type == SDL_EventType.SDL_KEYDOWN)
                {
                    if (ev.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE)
                        isRunning = false;
                    else if (ev.key.keysym.sym == SDL_Keycode.SDLK_UP)
                    {
                        scale = Math.Min(scale + 1, 10);
                        SDL_SetWindowSize(window, texW * scale, texH * scale);
                    }
                    else if (ev.key.keysym.sym == SDL_Keycode.SDLK_DOWN)
                    {
                        scale = Math.Max(scale - 1, 1);
                        SDL_SetWindowSize(window, texW * scale, texH * scale);
                    }
                    else if (ev.key.keysym.sym == SDL_Keycode.SDLK_SPACE)
                    {
                        isPaused = !isPaused;
                        SDL_PauseAudioDevice(audioDeviceId, isPaused ? 1 : 0);
                    }
                }
            }

            if (!isPaused && EnsureNextFrame())
            {
                var frame = decoder.Current;
                if (texW != frame.Width || texH != frame.Height)
                {
                    texW = frame.Width;
                    texH = frame.Height;
                    if (texture != IntPtr.Zero)
                        SDL_DestroyTexture(texture);
                    texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_RGB555, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, texW, texH);
                    SDL_SetTextureScaleMode(texture, SDL_ScaleMode.SDL_ScaleModeNearest);
                    SDL_SetWindowSize(window, texW * scale, texH * scale);
                }

                SDL_LockTexture(texture, IntPtr.Zero, out var texPixelPtr, out var texPitch);
                ushort* texPixels = (ushort*)texPixelPtr.ToPointer();
                int i = 0;
                for (int y = 0; y < texH; y++)
                {
                    ushort* texPixelsRow = texPixels + texPitch * y / 2;
                    for (int x = 0; x < texW; x++, texPixelsRow++)
                    {
                        *texPixelsRow = frame.Palette[frame.Color[i++]].Raw;
                    }
                }
                SDL_UnlockTexture(texture);

                hasFrame = false;
            }
            EnsureNextFrame();
            iii++;

            if (texture != IntPtr.Zero)
                SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
            SDL_RenderPresent(renderer);
            SDL_Delay((uint)(1000 * decoder.Current.Audio.Length / Decoder.AudioFrequency));
        }
    }
}
