using Raylib_cs;
using System.Net.NetworkInformation;
using System.Numerics;

class Program
{
    public struct RectangleCharacter
    {
        public Vector3 position;
        public Vector3 size;
        public Color color;
        public BoundingBox bounds;
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

    public struct Button
    {
        public Vector2 position;
        public Vector2 size;
        public Color color;
        public Color selectedColor;
        public string text;
    }

    //Screen
    const int SCREEN_WIDTH = 1240;
    const int SCREEN_HEIGTH = 720;
    const string TITLE = "Endless runner";
    const string CREDITS = "By Emiliano Martel";

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
    private static RectangleCharacter[] obstaclesArray = new RectangleCharacter[10];
    private static readonly Vector3[] SIZE_OBSTACLES_ARRAY = { new Vector3(5, 2.5f, 2), new Vector3(5, 2.5f, 2) };
    private static readonly Color[] COLOR_OBSTACLES_ARRAY = { Color.BLUE, Color.DARKBLUE };
    private static float timerObstacles = 0;
    const float MIN_TIME_OBSTACLE = 2f;
    const float MAX_TIME_OBSTACLE = 4f;
    private static float timeBetweenObstacles = 2;

    //Coins
    private static SphereCharacter[] coinsArray = new SphereCharacter[10];
    private static readonly float COIN_SIZE = 0.5f;
    private static readonly Color COIN_COLOR = Color.YELLOW;
    const int COIN_SCORE = 10;
    const float MIN_TIME_COIN = 2f;
    const float MAX_TIME_COIN = 4f;
    private static float timerCoins = 0;
    private static float timeBetweenCoins = 2;

    //Ground
    private static Vector3 groundPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private static Vector2 groundSize = new Vector2(128.0f, 700.0f);

    //LimitWalls
    private static RectangleCharacter wallLeft;
    private static RectangleCharacter wallRight;
    private static RectangleCharacter wallWin;
    private static readonly Vector3 WALL_WIN_SIZE = new Vector3(20.0f, 5.0f, 1.0f);
    private static readonly Vector3 WALL_LIMIT_SIZE = new Vector3(1.0f, 5.0f, 700.0f);
    private static Color colorWall = Color.RED;
    private static Color winWallColor = Color.GREEN;
    private static readonly Vector3 WALL_WIN_POSITION = new Vector3(0.0f, 2.5f, -200.0f);
    private static readonly Vector3 WALL_LEFT_POSITION = new Vector3(-10.0f, 2.5f, 0.0f);
    private static readonly Vector3 WALL_RIGHT_POSITION = new Vector3(10.0f, 2.5f, 0.0f);

    //Buttons
    private static Button playButton;
    private static Button rePlayButton;
    private static Button scoreButton;
    private static Button menuButton;
    private static readonly Color SELECTED_COLOR = Color.LIGHTGRAY;
    private static readonly Color COLOR_BUTTON = Color.GRAY;

    private static string endText = "";

    //Bools menu and gameplay
    private static bool gameStart = false;
    private static bool isMenu = true;
    private static bool endGame = false;
    private static bool isScoreMenu = false;

    //Score
    private static int score = 0;
    const int DISTANCE_INCREASE_SCORE = 10;
    const int INCREASE_SCORE = 1;
    private static float distance;
    private static int bestScore = 0;

    //Path
    private static string path = Path.Combine(Environment.CurrentDirectory,@"data.txt");

    public static void Main()
    {
        Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGTH, "Endless Runner");
        Raylib.SetTargetFPS(60);

        if (File.Exists(path))
        {
            int num;
            if(int.TryParse(File.ReadAllText(path), out num))
            {
                bestScore = num;
            }
        }

        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.FovY = 60.0f;
        camera.Projection = CameraProjection.CAMERA_PERSPECTIVE;

        //Instanciate player
        player.position = ORIGINAL_CHARACTER_POSITION;
        player.size = PLAYER_SIZE;
        player.color = Color.PURPLE;
        player.lineColor = Color.DARKPURPLE;

        //Instanciate walls
        wallLeft.position = WALL_LEFT_POSITION;
        wallLeft.size = WALL_LIMIT_SIZE;
        wallLeft.color = colorWall;
        wallLeft.bounds = RectangleBounds(wallLeft);
        wallRight.position = WALL_RIGHT_POSITION;
        wallRight.size = WALL_LIMIT_SIZE;
        wallRight.color = colorWall;
        wallRight.bounds = RectangleBounds(wallRight);
        wallWin.position = WALL_WIN_POSITION;
        wallWin.size = WALL_WIN_SIZE;
        wallWin.color = winWallColor;
        wallWin.bounds = RectangleBounds(wallWin);

        //Buttons
        playButton.position = new Vector2(250, 250);
        playButton.size = new Vector2(200, 80);
        playButton.color = COLOR_BUTTON;
        playButton.selectedColor = SELECTED_COLOR;
        playButton.text = "Play";

        rePlayButton.position = new Vector2(250, 250);
        rePlayButton.size = new Vector2(200, 80);
        rePlayButton.color = COLOR_BUTTON;
        rePlayButton.selectedColor = SELECTED_COLOR;
        rePlayButton.text = "Re play";

        scoreButton.position = new Vector2(250, 400);
        scoreButton.size = new Vector2(200, 80);
        scoreButton.color = COLOR_BUTTON;
        scoreButton.selectedColor = SELECTED_COLOR;
        scoreButton.text = "Scores";

        menuButton.position = new Vector2(250, 400);
        menuButton.size = new Vector2(200, 80);
        menuButton.color = COLOR_BUTTON;
        menuButton.selectedColor = SELECTED_COLOR;
        menuButton.text = "Menu";

        //Instanciate Obstacles
        CreateObstacles();

        //Instanciate Coins
        CreateCoins();

        //Score
        distance = player.position.Z;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.ClearBackground(Color.SKYBLUE);

            delta = Raylib.GetFrameTime();

            //DrawGameplay
            Raylib.BeginMode3D(camera);

            if (gameStart)
            {
                isMenu = false;
                Gameplay();
            }

            // Draw ground
            Raylib.DrawPlane(groundPosition, groundSize, Color.LIGHTGRAY);

            // Draw a walls
            Raylib.DrawCube(wallLeft.position, wallLeft.size.X, wallLeft.size.Y, wallLeft.size.Z, wallLeft.color);
            Raylib.DrawCube(wallRight.position, wallLeft.size.X, wallLeft.size.Y, wallLeft.size.Z, wallRight.color);
            Raylib.DrawCube(wallWin.position, wallWin.size.X, wallWin.size.Y, wallWin.size.Z, wallWin.color);

            //CheckCollisions
            if (Raylib.CheckCollisionBoxSphere(wallRight.bounds, player.position, player.size) || Raylib.CheckCollisionBoxSphere(wallLeft.bounds, player.position, player.size))
            {
                WinOrLoseLogic(false);
            }
            if (Raylib.CheckCollisionBoxSphere(wallWin.bounds, player.position, player.size))
            {
                WinOrLoseLogic(true);
            }
            Raylib.EndMode3D();

            //Draw UI
            Raylib.BeginDrawing();

            DrawUI();

            Raylib.EndDrawing();
        }
        ReWriteScore();
        Raylib.CloseWindow();
    }

    private static void DrawUI()
    {
        if (gameStart)
        {
            Raylib.DrawText(score.ToString(), 20, 100, 50, Color.WHITE);
        }
        else
        {
            if (isMenu)
            {
                Menu();
            }
            else if (endGame)
            {
                EndGameMenu();
            }
            else if (isScoreMenu)
            {
                ScoreMenu();
            }
        }
    }

    private static void ScoreMenu()
    {
        Raylib.DrawText(TITLE, (SCREEN_WIDTH - 300) / 2, 50, 50, Color.WHITE);
        Raylib.DrawText("Best score", (SCREEN_WIDTH - 300) / 2, 200, 50, Color.WHITE);
        Raylib.DrawText(bestScore.ToString(), (SCREEN_WIDTH - 300) / 2, 300, 50, Color.WHITE);
        isMenu = ButtonLogic(menuButton);
    }

    private static bool ButtonLogic(Button buttonStruct)
    {
        Rectangle button = new Rectangle(buttonStruct.position.X, buttonStruct.position.Y, buttonStruct.size.X, buttonStruct.size.Y);

        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), button))
        {
            Raylib.DrawRectangleRec(button, buttonStruct.selectedColor);
            Raylib.DrawText(buttonStruct.text, (int)buttonStruct.position.X, (int)buttonStruct.position.Y, 50, Color.WHITE);
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                return true;
            }
        }
        else
        {
            Raylib.DrawRectangleRec(button, buttonStruct.color);
            Raylib.DrawText(buttonStruct.text, (int)buttonStruct.position.X, (int)buttonStruct.position.Y, 50, Color.WHITE);
        }
        return false;
    }

    private static void EndGameMenu()
    {
        Raylib.DrawText(endText, (SCREEN_WIDTH - 300) / 2, 50, 50, Color.WHITE);
        Raylib.DrawText($"Your score is {score}", (SCREEN_WIDTH - 300) / 2, 100, 50, Color.WHITE);
        if (score > bestScore)
        {
            Raylib.DrawText("New best score", (SCREEN_WIDTH - 300) / 2, 150, 50, Color.WHITE);
        }
        gameStart = ButtonLogic(rePlayButton);
        if (gameStart)
        {
            ResetGame();
            endGame = false;
        }
        isMenu = ButtonLogic(menuButton);
        if (isMenu)
        {
            endGame = false;
            isScoreMenu = false;
        }
    }

    private static void Menu()
    {
        Raylib.DrawText(TITLE, (SCREEN_WIDTH - 300) / 2, 50, 50, Color.WHITE);
        Raylib.DrawText(CREDITS, 50, (SCREEN_WIDTH - 200) / 2, 50, Color.WHITE);
        gameStart = ButtonLogic(playButton);
        if (gameStart)
        {
            ResetGame();
            endGame = false;
        }
        isScoreMenu = ButtonLogic(scoreButton);
        if (isScoreMenu)
        {
            isMenu = false;
        }
    }

    private static void Gameplay()
    {
        SpawnObstacles();
        SpawnCoins();
        UpdatePlayer();
        UpdateCameraPosition();
        UpdateObstacles();
        UpdateCoins();
        UpdateScore();
    }

    private static void DrawGameSphere(SphereCharacter character)
    {
        Raylib.DrawSphere(character.position, character.size, character.color);
        Raylib.DrawSphereWires(character.position, character.size, 8, 8, character.lineColor);
    }

    private static void UpdateScore()
    {
        if (player.position.Z <= distance - DISTANCE_INCREASE_SCORE)
        {
            distance = player.position.Z;
            score += INCREASE_SCORE;
        }
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

    private static void CreateCoins()
    {
        for (int i = 0; i < coinsArray.Length; i++)
        {
            coinsArray[i].size = COIN_SIZE;
            coinsArray[i].color = COIN_COLOR;
        }
    }

    private static void UpdateCoins()
    {
        for (int i = 0; i < coinsArray.Length; i++)
        {
            if (coinsArray[i].isActive)
            {
                coinsArray[i].position.Z += delta * PLAYER_SPEED;
                DrawGameSphere(coinsArray[i]);
                if (Raylib.CheckCollisionSpheres(player.position, player.size, coinsArray[i].position, coinsArray[i].size))
                {
                    score += COIN_SCORE;
                    coinsArray[i].isActive = false;
                }
                if (coinsArray[i].position.Z >= player.position.Z)
                {
                    coinsArray[i].isActive = false;
                    coinsArray[i].position = player.position + new Vector3(0, 0, DIFF_Z_SPAWN);
                }
            }
        }
    }

    private static void CreateObstacles()
    {
        for (int i = 0; i < obstaclesArray.Length; i++)
        {
            int randomIndex = random.Next(0, SIZE_OBSTACLES_ARRAY.Length);
            obstaclesArray[i].size = SIZE_OBSTACLES_ARRAY[randomIndex];
        }
    }

    private static void UpdateObstacles()
    {
        for (int i = 0; i < obstaclesArray.Length; i++)
        {
            if (obstaclesArray[i].isActive)
            {
                obstaclesArray[i].position.Z += delta * PLAYER_SPEED;
                Raylib.DrawCube(obstaclesArray[i].position, obstaclesArray[i].size.X, obstaclesArray[i].size.Y, obstaclesArray[i].size.Z, obstaclesArray[i].color);
                obstaclesArray[i].bounds = RectangleBounds(obstaclesArray[i]);
                if (Raylib.CheckCollisionBoxSphere(obstaclesArray[i].bounds, player.position, player.size))
                {
                    WinOrLoseLogic(false);
                }
            }
            if (obstaclesArray[i].position.Z >= player.position.Z)
            {
                obstaclesArray[i].isActive = false;
                obstaclesArray[i].position = player.position + new Vector3(0, 0, DIFF_Z_SPAWN);
            }
        }
    }

    private static void WinOrLoseLogic(bool isWinner)
    {
        endGame = true;
        gameStart = false;
        if (isWinner)
        {
            endText = "You win.";
        }
        else
        {
            endText = "You lose.";
        }
        if (score > bestScore)
        {
            bestScore = score;
        }
    }

    private static void ReWriteScore()
    {
        try
        {
            File.WriteAllText(path, bestScore.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al intentar escribir en el archivo: " + ex.Message);
        }
    }

    private static void ResetGame()
    {
        player.position = ORIGINAL_CHARACTER_POSITION;
        distance = player.position.Z;
        for (int i = 0; i < obstaclesArray.Length; i++)
        {
            if (obstaclesArray[i].isActive)
            {
                obstaclesArray[i].isActive = false;
            }
        }
        for (int i = 0; i < coinsArray.Length; i++)
        {
            if (coinsArray[i].isActive)
            {
                coinsArray[i].isActive = false;
            }
        }
        score = 0;
    }

    private static void SpawnCoins()
    {
        timerCoins += Raylib.GetFrameTime();
        if (timerCoins >= timeBetweenCoins)
        {
            timeBetweenCoins = (float)(random.NextDouble() * (MIN_TIME_COIN - MAX_TIME_COIN) + MIN_TIME_COIN);
            timerCoins = 0;
            int positionX = random.Next((int)wallLeft.position.X, (int)wallRight.position.X);
            Vector3 positionObstacle = new Vector3(positionX, 1f, player.position.Z - DIFF_Z_SPAWN);
            for (int i = 0; i < coinsArray.Length; i++)
            {
                if (!coinsArray[i].isActive)
                {
                    coinsArray[i].isActive = true;
                    coinsArray[i].position = positionObstacle;
                    return;
                }
            }
        }
    }

    private static void SpawnObstacles()
    {
        timerObstacles += Raylib.GetFrameTime();
        if (timerObstacles >= timeBetweenObstacles)
        {
            timeBetweenObstacles = (float)(random.NextDouble() * (MIN_TIME_OBSTACLE - MAX_TIME_OBSTACLE) + MIN_TIME_OBSTACLE);
            timerObstacles = 0;
            int positionX = random.Next((int)wallLeft.position.X, (int)wallRight.position.X);
            Vector3 positionObstacle = new Vector3(positionX, 1f, player.position.Z - DIFF_Z_SPAWN);
            for (int i = 0; i < obstaclesArray.Length; i++)
            {
                int index = random.Next(0, COLOR_OBSTACLES_ARRAY.Length);
                if (!obstaclesArray[i].isActive)
                {
                    obstaclesArray[i].isActive = true;
                    obstaclesArray[i].position = positionObstacle;
                    obstaclesArray[i].color = COLOR_OBSTACLES_ARRAY[index];
                    return;
                }
            }
        }
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