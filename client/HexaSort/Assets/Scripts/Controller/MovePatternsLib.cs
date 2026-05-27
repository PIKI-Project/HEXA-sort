namespace Controller
{
    public static class MovePatternsLib
    {
        public static readonly MovePattern[] Patterns =
        {
            // EASY
            new(0.05f, 1, 1),
            new(0.19f, 1, 1, 1),
            new(0.29f, 2, 2, 1, 1, 3, 3),
            new(0.31f, 2, 2, 1, 1, 1, 1),
            new(0.33f, 2, 2, 2, 4, 3, 3),
            new(0.35f, 3, 3, 3, 2, 1, 1),
            new(0.37f, 3, 3, 3, 3, 2, 2),


            // MID
            new(0.41f, 1, 1, 3, 2, 2),
            new(0.43f, 1, 1, 3, 2, 2),
            new(0.45f, 1, 1, 3, 3, 3),
            new(0.47f, 2, 2, 3, 3, 1),
            new(0.49f, 2, 2, 2, 2, 3),
            new(0.51f, 2, 2, 2, 2, 2),
            new(0.57f, 1, 1, 2, 3),
            new(0.61f, 3, 3, 1, 1),
            new(0.63f, 1, 1, 3, 2),
            new(0.65f, 1, 2, 2, 3),


            // HARD
            new(0.71f, 3, 1, 3),
            new(0.73f, 3, 1, 2),
            new(0.75f, 1, 2, 1, 3),
            new(0.79f, 1, 2, 3),
            new(0.81f, 1, 4, 3, 2),
            new(0.83f, 1, 2, 3, 3, 4),
            new(0.85f, 1, 1, 3, 1, 2),
            new(0.87f, 1, 1, 3, 2, 2),
            new(0.89f, 2, 1, 3, 3),
            new(0.93f, 1, 1, 2, 2, 4, 3),
            new(0.95f, 2, 2, 1, 3, 2),
            new(0.97f, 1, 2, 1, 5),
            new(0.99f, 5, 4, 2, 2, 1, 1),
            new(1.00f, 1, 2, 3, 4, 5, 1)
        };
    }
}
