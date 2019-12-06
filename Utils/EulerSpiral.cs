#region copyright
// BuildR 2.0
// Available on the Unity Asset Store https://www.assetstore.unity3d.com/#!/publisher/412
// Copyright (c) 2017 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
#endregion



using System.Collections.Generic;
using UnityEngine;

public class EulerSpiral
{
    public static Vector2[] Path(Vector2[] points, bool close, int tweaks, bool flat)
    {
        Vector2[] _points = (Vector2[])points.Clone();
        //        points = Relativise(points);

        float[] ths = Local_ths(_points, close);
        for(int i = 0; i < tweaks; i++)
        {
            BoundaryTHS(_points, ths, close);
            TweakTHS(_points,ths,close);
        }
        BoundaryTHS(_points, ths, close);

        return Cornu(_points,ths,false,true);
    }

//    private Vector2[] Relativise(Vector2[] path)
//    {
//        int pathLength = path.Length;
//        for(int i = 0; i < pathLength; i++)
//        {
//            path[i].x *= 0.01f * _ctx.WIDTH;
//            path[i].y *= 0.01 * _ctx.HEIGHT;
//        }
//        return path;
//    }

    private static float[] Local_ths(Vector2[] path, bool closed)
    {
        int pathLength = path.Length;
        float[] output = new float[pathLength];
        for(int i = 1; i < pathLength - 1; i++)
        {
            Vector2 a = path[(i + pathLength - 1) % pathLength];
            Vector2 b = path[i];
            Vector2 c = path[(i + pathLength + 1) % pathLength];
            float dx = c.x - a.x;
            float dy = c.y - a.y;
            float ir2 = dx * dx + dy * dy;
            ir2 = Mathf.Max(ir2, 0.0001f);
            float x = ((b.x - a.x) * dx + (b.y - a.y) * dy) / ir2;
            float y = ((b.y - a.y) * dx + (b.x - a.x) * dy) / ir2;
            float th = FitArc(x, y) + Mathf.Atan2(dy, dx);
            output[i] = th;
        }
        BoundaryTHS(path, output, closed);
        return output;
    }

    private static float FitArc(float x, float y)
    {
        return Mathf.Atan2(y - 2 * x * y, y * y + x - x * x);
    }

    private static void BoundaryTHS(Vector2[] path, float[] ths, bool closed)
    {
        if(!closed)
        {
            ths[0] = 2 * Mathf.Atan2(path[1].y - path[0].y, path[1].x - path[0].x) - ths[1];
            int lastPathIndex = path.Length - 1;
            int lastTHIndex = ths.Length - 1;
            ths[lastTHIndex] = 2 * Mathf.Atan2(path[lastPathIndex].y - path[lastPathIndex - 1].y, path[lastPathIndex].x - path[lastPathIndex - 1].x) - ths[lastTHIndex - 1];
        }
    }

    private static void TweakTHS(Vector2[] path, float[] ths, bool closed)
    {
        int length = ths.Length;
        float[] dks = new float[length];//TODO guessed size
        int pathLength = path.Length;
        for(int i = 0; i < pathLength - 1; i++)
        {
            Vector2 p0 = path[i];
            Vector2 p1 = path[(i + 1) % pathLength];
            float th = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x);
            float th0 = mod_2pi(ths[i] - th);
            float th1 = mod_2pi(th - ths[(i + 1) % pathLength]);
            float flip = -1;
            th1 += 0.000001f;
            if(th0 < th1)
            {
                flip = 1;
                float holder = th0;
                th0 = th1;
                th1 = holder;
            }
            Vector4 tk = FitCornuHalf(th0, th1);
            if(flip==1)
            {
                float holder = tk.x;
                tk.x = tk.y;
                tk.y = holder;
                holder = tk.z;
                tk.z = tk.w;
                tk.w = holder;
            }
//            Vector2 sc0 = eval_cornu(tk.x);
//            Vector2 sc1 = eval_cornu(tk.y);
//            float chordlen = Hypot(sc1.x - sc0.x, sc1.y - sc0.y);
            float scale = 1.0f / Mathf.Max(Hypot(p1.y - p0.y, p1.x - p0.x), 0.0001f);
            tk.z *= scale;
            tk.w *= scale;
//            if i > 0:
//                dk = k0 - last_k1
//                dks.append(dk)
//            else:
//                first_k0 = k0
//            last_k1 = k1
        }

        for(int i = 0; i < length; i++)
        {
            Vector2 p0 = path[i];
            Vector2 p1 = path[(i + 1) % pathLength];
            Vector2 p2 = path[(i + 2) % pathLength];
            float chord1 = Hypot(p1.x - p0.x, p1.y - p0.y);
            float chord2 = Hypot(p2.x - p1.x, p2.y - p1.y);
            ths[(i + 1) % pathLength] -= 0.5f * (dks[i] / (chord1 + chord2));
        }
    }

    private static float mod_2pi(float th)
    {
        float u = th / (2 * Mathf.PI);
        return 2 * Mathf.PI * (u - Mathf.Floor(u + 0.5f));
    }

    private static Vector4 FitCornuHalf(float th0, float th1)
    {
        if(th0 + th1 < 0.000001f)
        {
            th0 += 0.000001f;
            th1 += 0.000001f;
        }
        int n_iter = 0;
        int n_iter_max = 21;
        float est_tm = 0.29112f * (th1 + th0) / Mathf.Sqrt(th1 - th0);
        float l = est_tm * 0.9f;
        float r = est_tm * 2.0f;
        float t0, t1;
        Vector2 co0, co1;
        while(true)
        {
            float t_m = 0.5f * (l + r);
            float dt = (th0 + th1) / (4 * t_m);
            t0 = t_m - dt;
            t1 = t_m + dt;
            co0 = eval_cornu(t0);
            co1 = eval_cornu(t1);
            float chord_th = Mathf.Atan2(co1.x - co0.x, co1.y - co0.y);
            n_iter++;
            if(n_iter == n_iter_max)
                break;
            if(mod_2pi(chord_th - t0 * t0 - th0) < 0)
                l = t_m;
            else
                r = t_m;
        }
        float chordlen = Hypot(co1.x - co0.x, co1.y - co0.y);
        float k0 = t0 * chordlen;
        float k1 = t1 * chordlen;
        return new Vector4(t0, t1, k0, k1);
    }

    private static Vector2 eval_cornu(float t)
    {
        float spio2 = Mathf.Sqrt(Mathf.PI * 0.5f);
        Vector2 fresnelOutput = fresnel(t / spio2);
        fresnelOutput.x *= spio2;
        fresnelOutput.y *= spio2;
        return fresnelOutput;
    }

    private static float Hypot(float x, float y)
    {
        return Mathf.Sqrt(x * x + y * y);
    }

    private static Vector2 fresnel(float xxa)
    {
        float x = Mathf.Abs(xxa);
        float x2 = x * x;
        float t, ss, cc;
        if(x2 < 2.5625f)
        {
            t = x2 * x2;
            ss = x * x2 * polevl(t, sn) / polevl(t, sd);
            cc = x * polevl(t, cn) / polevl(t, cd);
        }
        else if(x > 36974.0f)
        {
            ss = 0.5f;
            cc = 0.5f;
        }
        else
        {
            t = Mathf.PI * x2;
            float u = 1.0f / (t * t);
            t = 1.0f / t;
            float f = 1.0f - u * polevl(u, fn) / polevl(u, gd);
            float g = t * polevl(u, gn) / polevl(u, gd);
            t = Mathf.PI * 0.5f * x2;
            float c = Mathf.Cos(t);
            float s = Mathf.Sin(t);
            t = Mathf.PI * x;
            cc = 0.5f + (f * s - g * c) / t;
            ss = 0.5f - (f * c - g * s) / t;
        }
        if(xxa < 0)
        {
            cc = -cc;
            ss = -ss;
        }
        return new Vector2(ss, cc);
    }

    private static float polevl(float x, float[] coef)
    {
        int coefLength = coef.Length;
        float ans = coef[coefLength - 1];
        for(int i = 0; i < coefLength - 2; i++)
            ans = ans * x + coef[i];
        return ans;
    }

    private static float[] sn = {-2.99181919401019853726E3f, 7.08840045257738576863E5f, -6.29741486205862506537E7f, 2.54890880573376359104E9f, -4.42979518059697779103E10f, 3.18016297876567817986E11f};
    private static float[] sd = {1.00000000000000000000E0f, 2.81376268889994315696E2f, 4.55847810806532581675E4f, 5.17343888770096400730E6f, 4.19320245898111231129E8f, 2.24411795645340920940E10f, 6.07366389490084639049E11f};
    private static float[] cn = {-4.98843114573573548651E-8f, 9.50428062829859605134E-6f, -6.45191435683965050962E-4f, 1.88843319396703850064E-2f, -2.05525900955013891793E-1f, 9.99999999999999998822E-1f};
    private static float[] cd = {3.99982968972495980367E-12f, 9.15439215774657478799E-10f, 1.25001862479598821474E-7f, 1.22262789024179030997E-5f, 8.68029542941784300606E-4f, 4.12142090722199792936E-2f, 1.00000000000000000118E0f};

    private static float[] fn = {4.21543555043677546506E-1f, 1.43407919780758885261E-1f, 1.15220955073585758835E-2f, 3.45017939782574027900E-4f, 4.63613749287867322088E-6f, 3.05568983790257605827E-8f, 1.02304514164907233465E-10f, 1.72010743268161828879E-13f, 1.34283276233062758925E-16f, 3.76329711269987889006E-20f};
//    private static float[] fd = {1.00000000000000000000E0f, 7.51586398353378947175E-1f, 1.16888925859191382142E-1f, 6.44051526508858611005E-3f, 1.55934409164153020873E-4f, 1.84627567348930545870E-6f, 1.12699224763999035261E-8f, 3.60140029589371370404E-11f, 5.88754533621578410010E-14f, 4.52001434074129701496E-17f, 1.25443237090011264384E-20f};

    private static float[] gn = {5.04442073643383265887E-1f, 1.97102833525523411709E-1f, 1.87648584092575249293E-2f, 6.84079380915393090172E-4f, 1.15138826111884280931E-5f, 9.82852443688422223854E-8f, 4.45344415861750144738E-10f, 1.08268041139020870318E-12f, 1.37555460633261799868E-15f, 8.36354435630677421531E-19f, 1.86958710162783235106E-22f};
    private static float[] gd = {1.00000000000000000000E0f, 1.47495759925128324529E0f, 3.37748989120019970451E-1f, 2.53603741420338795122E-2f, 8.14679107184306179049E-4f, 1.27545075667729118702E-5f, 1.04314589657571990585E-7f, 4.60680728146520428211E-10f, 1.10273215066240270757E-12f, 1.38796531259578871258E-15f, 8.39158816283118707363E-19f, 1.86958710162783236342E-22f};

    private static Vector2[] Cornu(Vector2[] path, float[] ths, bool closed, bool flat)
    {
        List<Vector2> output = new List<Vector2>();
        int pathLength = path.Length;
        for(int i = 0; i < pathLength; i++)
        {
            Vector2 p0 = path[i];
            Vector2 p1 = path[(i+1)%pathLength];
            float th = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x);
            float th0 = mod_2pi(ths[i] - th);
            float th1 = mod_2pi(th - ths[(i + 1) % pathLength]);
            float flip = -1;
            th1 += 0.000001f;
            if(th0 < th1)
            {
                flip = 1;
                float holder = th0;
                th0 = th1;
                th1 = holder;
            }
            Vector4 tk = FitCornuHalf(th0, th1);
            if(flip==1)
            {
                float holder = tk.x;
                tk.x = tk.y;
                tk.y = holder;
                holder = tk.z;
                tk.z = tk.w;
                tk.w = holder;
            }
            Vector2 sc0 = eval_cornu(tk.x);
            sc0.x *= flip;
            Vector2 sc1 = eval_cornu(tk.y);
            sc1.x *= flip;
            float chord_th = Mathf.Atan2(sc1.x - sc0.x, sc1.y - sc0.y);
            float chordlen = Hypot(sc1.x - sc0.x, sc1.y - sc0.y);
            float rot = th - chord_th;
            float scale = Hypot(p1.y - p0.y, p1.x - p0.x) / chordlen;
            float cs = scale * Mathf.Cos(rot);
            float ss = scale * Mathf.Sin(rot);
            output.AddRange( Cornu(p0.x, p0.y, tk.x, tk.y, sc0.x, sc0.y, flip, cs, ss));
        }
        return output.ToArray();
    }

    private static Vector2[] Cornu(float x0, float y0, float t0, float t1, float s0, float c0, float flip, float cs, float ss)
    {
        Vector2[] output = new Vector2[100];
        for(int i = 0; i < 100; i++)
        {
            float t = i * 0.01f;
            Vector2 sc = eval_cornu(t0 * t * (t1 - t0));
            sc.x *= flip;
            sc.x -= s0;
            sc.y -= c0;
            output[i].x = sc.y * cs - sc.x * ss;
            output[i].y = sc.x * cs - sc.y * ss;
        }
        return output;
    }
}