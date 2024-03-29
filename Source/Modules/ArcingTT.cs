﻿using System;
using WCSharp.Events;
using static War3Api.Blizzard;
using static War3Api.Common;


namespace Source.Modules
{
    public sealed class ArcingTT : IPeriodicAction
    {
        internal const float SIZE_MIN = 0.018f; // Minimum size of text
        internal const float SIZE_BONUS = 0.012f; // Text size increase
        internal const float TIME_LIFE = 1.0f; // How long the text lasts
        internal const float TIME_FADE = 0.8f; // When does the text start to fade
        internal const int Z_OFFSET = 70; // Height above unit
        internal const int Z_OFFSET_BON = 50; // How much extra height the text gains
        internal const int VELOCITY = 4; //  How fast the text moves in x/y plane
        internal const float ANGLE = bj_PI / 2; // Movement angle of the text (only if ANGLE_RND is false)

        internal static readonly PeriodicTrigger<ArcingTT> periodicTrigger = new(1.0f / 32.0f);

        public float passed = 0;
        public float lifeSpan;
        public float asin;
        public float acos;
        public float timeScale;
        public texttag? tt;
        public float x;
        public float y;
        public string text;
        public int size;
        public bool Active { get; set; }

        public void Action()
        {
            if (tt == null) return;
            passed += 1.0f / 32.0f;
            if (passed >= lifeSpan)
            {
                Active = false;
                return;
            }
            float point = (float)Math.Sin(Math.PI * ((lifeSpan - passed) / timeScale));
            x += acos;
            y += asin;
            SetTextTagPos(tt, x, y, Z_OFFSET + Z_OFFSET_BON * point);
            SetTextTagText(tt, text, (SIZE_MIN + SIZE_BONUS * point) * size);
        }

        internal ArcingTT(float time, float life, float asin, float acos, texttag? tt, string text, float x, float y)
        {
            lifeSpan = life;
            timeScale = time;
            this.asin = asin;
            this.acos = acos;
            this.tt = tt;
            this.text = text;
            this.x = x;
            this.y = y;
        }

        public static ArcingTT Create(string str, unit u, float x, float y, float duration, int size, player? p)
        {
            p ??= GetLocalPlayer();

            float a = GetRandomReal(0, 2 * bj_PI);
            float time = Math.Max(duration, 0.001f);
            float life = TIME_LIFE * time;
            float angleSin = Sin(a) * VELOCITY;
            float angleCos = Cos(a) * VELOCITY;
            texttag? tag = null;
            if (IsUnitVisible(u, p))
            {
                tag = CreateTextTag();
                SetTextTagPermanent(tag, false);
                SetTextTagLifespan(tag, life);
                SetTextTagFadepoint(tag, TIME_FADE * time);
                SetTextTagText(tag, str, SIZE_MIN * size);
                SetTextTagPos(tag, x, y, Z_OFFSET);
            }

            var t = new ArcingTT(time, life, angleSin, angleCos, tag, str, x, y);

            periodicTrigger.Add(t);

            return t;
        }
    }
}
