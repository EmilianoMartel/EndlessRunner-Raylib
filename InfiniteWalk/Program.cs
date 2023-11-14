using Raylib_cs;
using System.Numerics;

class Program
{
    public struct RectangleCharacter
    {
        public Vector3 position;
        public Vector3 size;
        public Color color;
        public bool isActive;
    }

    public struct SphereCharacter
    {
        public Vector3 position;
        public float size;
        public Color color;
        public Color lineColor;
        public bool isActive;
    }

    //Screen
    const int SCREEN_WIDTH = 1240;
    const int SCREEN_HEIGTH = 720;

    //Camera
    const float DIFF_CAMERA_Z = 5f;

    private const float PLAYER_SPEED = 10;

    public static Camera3D camera = new Camera3D();
    private static Random random = new Random();
    private static float delta;

    //Player
    private static SphereCharacter player;
    const float PLAYER_SIZE = 0.25f;
    private static readonly Vector3 ORIGINAL_CHARACTER_POSITION = new(0.0f, 1.0f, 16f);

    //Obstacles and coins
    const int DIFF_Z_SPAWN = 100;

    //Obstacles
    private static RectangleCharacter[] rectangleObstaclesArray = new RectangleCharacter[10];
    private static readonly Vector3[] sizeRectanglesArray = { new Vector3(5, 2.5f, 2), new Vector3(5, 2.5f, 2) };
    private static float timerObstacles = 0;
    private static int timeBetweenObstacles = 2;

    //Coins
    private static SphereCharacter[] sphereCoinsArray = new SphereCharacter[10];

    //Ground
    private static Vector3 groundPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private static Vector2 groundSize = new Vector2(128.0f, 128.0f);

    //LimitWalls
    private static RectangleCharacter wallLeft;
    private static RectangleCharacter wallRight;
    private static Vector3 wallSize = new Vector3(1.0f, 5.0f, 700.0f);
    private static Color colorWall = Color.RED;
    private static Vector3 wallLeftPosition = new Vector3(-10.0f, 2.5f, 0.0f);
    private static Vector3 wallRightPosition = new Vector3(10.0f, 2.5f, 0.0f);

    public static void Main()
    {
        Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGTH, "Endless Runner");
        Raylib.SetTargetFPS(60);

        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.FovY = 60.0f;
        camera.Projection = CameraProjection.CAMERA_PERSPECTIVE;

        //Instanciate player
        player.position = ORIGINAL_CHARACTER_POSITION;
        player.size = PLAYER_SIZE;
        player.color = Color.PURPLE;
        player.lineColor = Color.DARKPURPLE;

        //Instanciate walls
        wallLeft.position = wallLeftPosition;
        wallLeft.size = wallSize;
        wallLeft.color = colorWall;
        wallRight.position = wallRightPosition;
        wallRight.size = wallSize;
        wallRight.color = colorWall;

        //Instanciate Obstacles
        CreateObstacles();

        while (!Raylib.WindowShouldClose())
        {
            Raylib.ClearBackground(Color.RAYWHITE);

            delta = Raylib.GetFrameTime();

            //DrawGameplay
            Raylib.BeginMode3D(camera);

            UpdatePlayer();
            UpdateCameraPosition();


            // Draw ground
            Raylib.DrawPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(128.0f, 128.0f), Color.LIGHTGRAY);

            // Draw a walls
            Raylib.DrawCube(wallLeft.position, wallLeft.size.X, wallLeft.size.Y, wallLeft.size.Z, wallLeft.color);
            Raylib.DrawCube(wallRight.position, wallLeft.size.X, wallLeft.size.Y, wallLeft.size.Z, wallRight.color);

            Raylib.EndMode3D();

            //Draw UI
            Raylib.BeginDrawing();


            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    private static void DrawUI()
    {

    }

    private static void DrawGameSphere(SphereCharacter character)
    {
        Raylib.DrawSphere(character.position, character.size, character.color);
        Raylib.DrawSphereWires(character.position, character.size, 8, 8, character.lineColor);
    }

    private static void UpdatePlayer()
    {

        player.position.Z -= delta * PLAYER_SPEED;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
        {
            player.position.X += delta * PLAYER_SPEED;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
        {
            player.position.X -= delta * PLAYER_SPEED;
        }

        //Draw player
        DrawGameSphere(player);
    }

    private static void CreateObstacles()
    {
        for (int i = 0; i < rectangleObstaclesArray.Length; i++)
        {
            int randomIndex = random.Next(0, sizeRectanglesArray.Length);
            rectangleObstaclesArray[i].size = sizeRectanglesArray[randomIndex];
            rectangleObstaclesArray[i].color = colorWall;
        }
    }

    private static void UpdateObstacles()
    {
        for (int i = 0; i < rectangleObstaclesArray.Length; i++)
        {
            if (rectangleObstaclesArray[i].isActive)
            {
                rectangleObstaclesArray[i].position.Z += delta * PLAYER_SPEED;
            }
            if (rectangleObstaclesArray[i].position.Z > player.position.Z + DIFF_CAMERA_Z)
            {
                rectangleObstaclesArray[i].isActive = false;
                rectangleObstaclesArray[i].position = player.position + new Vector3(0,0,DIFF_Z_SPAWN);
            }
        }
    }

    private static void SpawnObstacles()
    {

    }

    private static void UpdateCameraPosition()
    {
        camera.Position = new Vector3(0, 2.0f, player.position.Z + DIFF_CAMERA_Z);
        camera.Target = new Vector3(0, 1f, player.position.Z);
    }

    private static BoundingBox RectangleBounds(RectangleCharacter rectangle)
    {
        return new BoundingBox(new Vector3(rectangle.position.X - rectangle.size.X / 2,
                                           rectangle.position.Y - rectangle.size.Y / 2,
                                           rectangle.position.Z - rectangle.size.Z / 2),
                               new Vector3(rectangle.position.X + rectangle.size.X / 2,
                                           rectangle.position.Y + rectangle.size.Y / 2,
                                           rectangle.position.Z + rectangle.size.Z / 2));
    }
}