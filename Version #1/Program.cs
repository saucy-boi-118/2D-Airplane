using System;
using System.Numerics;
using Raylib_cs;
using static Global;
class Global
{
    public const int WINW = 968;
    public const int WINH = 484;
    public const float FULL_CIRCLE = 2 * MathF.PI; // Full Circle in radians
    public const float TO_DEGREE = 180 / MathF.PI; // conversion to degree multiplier
    public const float TO_RADIAN = MathF.PI / 180; // conversion to radian multiplier
    public static readonly Vector2 CENTER = new(WINW / 2, WINH / 2);
}

class ParticleSystem(int MaxParticles)
{
    public struct Particles()
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
        public float Age; 
        public bool Alive;          
    }

    public Particles[] particles = new Particles[MaxParticles];
    public static Vector2 grav = new(0, -1500f);

    public static void OverWriteParticle(ref Particles[] p, Vector2 origin, int i, Random random, int size)
    {
        p[i].Position = origin; // set it to the origin

        // set the velocity | speed in a direction
        p[i].Velocity = Vector2.Normalize(new((random.NextSingle()+0.1f) * 2 - 1, random.NextSingle() * 2 - 1)) * ((random.NextSingle()+0.1f) * 100);

        p[i].Size = size; // size to seven

        p[i].Age = 0; // reset age

        p[i].Alive = true; // set alive to true
    }

    public static void UpdateParticleSMOKE(ref Particles[] p, Vector2 origin, int size, Vector2 dir, Random r, float dt, int Lifespan)
    {
            for (int i = 0; i < p.Length - 1; i++)
            {
                if (p[i].Alive == true)
                {
                    // Moving
                    p[i].Velocity += dir * dt; // add acceleration 
                    p[i].Position += p[i].Velocity * dt; // move with velocity

                    p[i].Size += 0.3f; // increase size

                    // make it fade
                    p[i].Color.A -= 4; if (p[i].Color.A < 4) {p[i].Alive = false;}

                    p[i].Age += dt; // increase age

                    if (p[i].Age > Lifespan) {p[i].Alive = false;}   
                }
                else if (p[i].Alive == false && r.NextSingle() < 0.1)
                {
                    p[i].Color = Color.Gray; // color to grey
                    OverWriteParticle(ref p, origin, i, r, size);
                }

                // Drawing
                Raylib.DrawCircleV(p[i].Position, p[i].Size, p[i].Color);
            }
}

    public static void UpdateParticleFIRE(ref Particles[] p, Vector2 origin, Vector2 dir, Random r, float dt, float Lifespan)
    {
            for (int i = 0; i < p.Length - 1; i++)
            {
                if (p[i].Alive == true)
                {
                    // Moving
                    p[i].Velocity += dir * dt; // add acceleration 
                    p[i].Position += p[i].Velocity * dt; // move with velocity

                    p[i].Size -= 0.1f; // increase size

                    // make it fade and turn it more red
                    p[i].Color.G -= 3; 
                    p[i].Color.A -= 8;
                    if (p[i].Color.A <= 4) {p[i].Alive = false;} 

                    p[i].Age += dt; // increase age

                    if (p[i].Age > Lifespan) {p[i].Alive = false;}   
                }
                else if (p[i].Alive == false && r.NextSingle() < 0.1f)
                {
                    p[i].Color = Color.Yellow; // color to grey
                    OverWriteParticle(ref p, origin, i, r, 10);
                }

                // Drawing
                Raylib.DrawCircleV(p[i].Position, p[i].Size, p[i].Color);
            }
    }

    public static void UpdateParticleEXPLODE(ref Particles[] p, Vector2 origin, Random r, float dt, Color color, int WINHEIGHT)
    {
        for (int i = 0; i < p.Length - 1; i++)
            {
                if (p[i].Alive == true)
                {
                    // Moving
                    p[i].Velocity -= grav * dt; // add acceleration 
                    p[i].Position += p[i].Velocity * dt; // move with velocity

                    p[i].Color.A -= 2; if (p[i].Color.A <= 4) {p[i].Alive = false;}

                    if (p[i].Position.Y >= WINHEIGHT+50) {p[i].Alive = false;}  
                }
                else if (p[i].Alive == false && r.NextSingle() < 0.1)
                {
                    p[i].Color = color; // color to grey
                    OverWriteParticle(ref p, origin, i, r, 7);
                    p[i].Velocity.Y -= 550;
                }

                // Drawing
                Raylib.DrawCircleV(p[i].Position, p[i].Size, p[i].Color);
            }
    }
}

class Program
{
    public static void Main()
    {
        // Initialization
        Raylib.InitWindow(WINW, WINH, "Airplane 2D");
        Raylib.SetTargetFPS(100);
        float dt;

        // Camera
        Camera2D camera = new()
        {
            Target = CENTER,
            Offset = CENTER,
            Rotation = 0f,
            Zoom = 1,
        };
        

        // PLANE MOVEMENT
        Vector2 position = CENTER;
        Vector2 offset = position;
        Vector2 direction = Vector2.Zero;
        Vector2 velocity = Vector2.Zero;
        float speed = 250f;

        // PLANE TURNING
        float angleDirection = 0; // in degrees

        // PLANE VISUALS
        Texture2D plane = Raylib.LoadTexture("plane.png");
        float width = plane.Width;
        float height = plane.Height;
        int scale = 7;
        Rectangle Source = new(0,0,width,height);
        Rectangle Dest = new(position,width/scale,height/scale);
        Vector2 Origin = new(width/(scale*2), height/(scale*2));

        // Screen
        Rectangle screen = new(25,25,WINW-25,WINH-25);
        float radius = width / scale;

        // Particles System
        ParticleSystem psSmoke = new(25);
        Random r = new();
        


        while(!Raylib.WindowShouldClose())
        {
            dt = Raylib.GetFrameTime(); // delta time

            // PLANE CONTROLS -> TURNING
            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) // Left
            {
                angleDirection -= 0.02f;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) // Right
            {
                angleDirection += 0.02f;
            }

            // Cap the angle
            if (angleDirection >= FULL_CIRCLE) {angleDirection = 0;}
            else if (angleDirection <= 0) {angleDirection += FULL_CIRCLE;}


            // PLANE MOVEMENT

            // calculate direction
            direction = new(MathF.Cos(angleDirection) * 1.5f, MathF.Sin(angleDirection) * 1.5f);

            // constantly update velocity
            velocity += direction * speed *dt;
            
            // adding friction
            velocity *= 0.99f;

            // update position
            position += velocity * dt; 
            Dest.Position = position;

            // Collision with screen
            if (!Raylib.CheckCollisionCircleRec(position, radius, screen))
            {
                velocity *= -0.75f;
            }

            // Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);
            Raylib.BeginMode2D(camera);

            // update particles
            ParticleSystem.UpdateParticleSMOKE(ref psSmoke.particles, position, 5, -direction, r, dt, 500);

            // Draw the plane
            Raylib.DrawTexturePro(plane, Source, Dest, Origin, (angleDirection * TO_DEGREE) + 90, Color.White);

            


            Raylib.EndMode2D();
            Raylib.EndDrawing();
        }

        // Unloading / Closing
        Raylib.UnloadTexture(plane);
        Raylib.CloseWindow();
    }
}