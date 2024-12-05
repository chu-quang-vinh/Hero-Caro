using ConsoleApp16;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using static ConsoleApp16.BoardManager;

namespace ConsoleApp16
{
    public class BoardManager
    {
        public enum CellState { Empty, Player1, Player2, Resource }

        public CellState[,] board = new CellState[10, 10];
        public bool IsValidMove(int x, int y)
        {
            return x >= 0 && x < 10 && y >= 0 && y < 10 && board[x, y] == CellState.Empty;
        }
        public void PlacePiece(int x, int y, CellState player)
        {
            if (IsValidMove(x, y))
            {
                board[x, y] = player;
                CheckWinCondition(x, y, player);
            }
        }
        public bool CheckWinCondition(int x, int y, CellState player)
        {
            return CountConsecutive(x, y, 1, 0, player) + CountConsecutive(x, y, -1, 0, player) >= 4 ||
                   CountConsecutive(x, y, 0, 1, player) + CountConsecutive(x, y, 0, -1, player) >= 4 ||
                   CountConsecutive(x, y, 1, 1, player) + CountConsecutive(x, y, -1, -1, player) >= 4 ||
                   CountConsecutive(x, y, 1, -1, player) + CountConsecutive(x, y, -1, 1, player) >= 4;
        }
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < 10 && y >= 0 && y < 10;
        }
        private int CountConsecutive(int x, int y, int dx, int dy, CellState player)
        {
            int count = 0;
            while (IsInBounds(x + dx, y + dy) && board[x + dx, y + dy] == player)
            {
                x += dx;
                y += dy;
                count++;
            }
            return count;
        }

    }
    public class ResourceManager
    {
        public int Mana { get; private set; }
        public int Rage { get; private set; }
        public int Sword { get; private set; }
        public enum ResourceType
        {
            Mana,
            Rage,
            Sword
        }

        public void AddResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Mana:
                    Mana = Math.Min(Mana + amount, 5); // Giới hạn tối đa 5 Mana
                    break;
                case ResourceType.Rage:
                    Rage = Math.Min(Rage + amount, 5); // Giới hạn tối đa 5 Rage
                    break;
                case ResourceType.Sword:
                    Sword += amount;
                    break;
            }
        }



        public void CheckAndAddResource(int x, int y, CellState player, CellState[,] board)
        {
            if (IsFourInRow(x, y, player, board))
            {
                AddResource(ResourceType.Mana, 1);
            }
        }

        private bool IsFourInRow(int x, int y, CellState player, CellState[,] board)
        {
            // Logic kiểm tra chuỗi 4 quân cờ.
            return CountConsecutive(x, y, 1, 0, player, board) >= 3; // Chỉnh điều kiện phù hợp.
        }

        private int CountConsecutive(int x, int y, int dx, int dy, CellState player, CellState[,] board)
        {
            int count = 0;
            while (x >= 0 && x < 10 && y >= 0 && y < 10 && board[x, y] == player)
            {
                x += dx;
                y += dy;
                count++;
            }
            return count;
        }
    }
    public class SkillManager
    {
        private BoardManager boardManager;
        public enum SkillType
        {
            BlackHole,
            DestroyArea,
            Heal,
            RageBoost
        }

        public SkillManager(BoardManager boardManager)
        {
            this.boardManager = boardManager;
        }

        public void UseSkill(SkillType skill, Player player)
        {
            switch (skill)
            {
                case SkillType.DestroyArea:
                    if (player.Mana >= 2 && player.Rage >= 2)
                    {
                        player.Mana -= 2;
                        player.Rage -= 2;
                        DestroyArea(5, 5); // Phá hủy khu vực 3x3
                    }
                    break;
                case SkillType.Heal:
                    if (player.Mana >= 3)
                    {
                        player.Mana -= 3;
                        player.Health += 25; // Hồi 25 máu
                    }
                    break;
                case SkillType.RageBoost:
                    if (player.Rage >= 3)
                    {
                        player.Rage -= 3;
                        player.BoostedDamageTurns = 3; // Kích hoạt hiệu ứng tăng sát thương
                    }
                    break;
            }
        }




        public void DestroyArea(int centerX, int centerY)
        {
            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                for (int y = centerY - 1; y <= centerY + 1; y++)
                {
                    if (boardManager.IsInBounds(x, y))
                        boardManager.board[x, y] = BoardManager.CellState.Empty;
                }
            }
        }

        public void GenerateResources(int turnCount)
        {
            if (turnCount % 5 == 0) // Mỗi 5 lượt
            {
                Random random = new Random();
                int resourceCount = random.Next(1, 4); // 1 đến 3 ô tài nguyên
                for (int i = 0; i < resourceCount; i++)
                {
                    int x, y;
                    do
                    {
                        x = random.Next(0, 10);
                        y = random.Next(0, 10);
                    } while (boardManager.board[x, y] != BoardManager.CellState.Empty);

                    boardManager.board[x, y] = BoardManager.CellState.Resource;
                }
            }
        }



    }
    public class Player
    {
        public int Mana { get; set; }      // Lượng Mana của người chơi
        public int Rage { get; set; }      // Lượng Rage của người chơi
        public int Health { get; set; }    // Máu của người chơi
        public int BoostedDamageTurns { get; set; } // Số lượt còn lại của tăng sát thương
        
        public Player()
        {
            Mana = 0;        // Khởi tạo Mana
            Rage = 0;        // Khởi tạo Rage
            Health = 100;    // Khởi tạo máu tối đa
        }
    }
    class GameManager
    {
        public void SaveGame(string filePath, BoardManager boardManager, Player player1, Player player2)
        {
            var gameState = new
            {
                Board = boardManager.board,
                Player1 = new { player1.Mana, player1.Rage, player1.Health },
                Player2 = new { player2.Mana, player2.Rage, player2.Health }
            };

            string json = JsonConvert.SerializeObject(gameState, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public void LoadGame(string filePath, BoardManager boardManager, Player player1, Player player2)
        {
            string json = File.ReadAllText(filePath);
            dynamic gameState = JsonConvert.DeserializeObject(json);

            boardManager.board = gameState.Board.ToObject<BoardManager.CellState[,]>();
            player1.Mana = gameState.Player1.Mana;
            player1.Rage = gameState.Player1.Rage;
            player1.Health = gameState.Player1.Health;
            player2.Mana = gameState.Player2.Mana;
            player2.Rage = gameState.Player2.Rage;
            player2.Health = gameState.Player2.Health;
        }
        
        
        private int turnCount = 0;
        private BoardManager boardManager;
        private Player player1, player2;
        private SkillManager skillManager;
        private UIManager uiManager;

        public GameManager()
        {
            boardManager = new BoardManager();
            player1 = new Player();
            player2 = new Player();
            skillManager = new SkillManager(boardManager);
            uiManager = new UIManager();
        }

        public void StartGame()
        {
            while (true)
            {
                uiManager.DisplayBoard(boardManager);
                Console.WriteLine($"Turn {turnCount + 1}: Player {(turnCount % 2 == 0 ? 1 : 2)}'s turn.");

                int x, y;
                Console.Write("Enter X coordinate: ");
                x = int.Parse(Console.ReadLine());
                Console.Write("Enter Y coordinate: ");
                y = int.Parse(Console.ReadLine());

                var currentPlayer = turnCount % 2 == 0 ? player1 : player2;
                if (boardManager.IsValidMove(x, y))
                {
                    boardManager.PlacePiece(x, y, turnCount % 2 == 0 ? BoardManager.CellState.Player1 : BoardManager.CellState.Player2);
                    turnCount++;

                    // Kiểm tra chiến thắng
                    if (boardManager.CheckWinCondition(x, y, turnCount % 2 == 0 ? BoardManager.CellState.Player1 : BoardManager.CellState.Player2))
                    {
                        Console.WriteLine($"Player {(turnCount % 2 == 0 ? 1 : 2)} wins!");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid move. Try again.");
                }
            }
        }

    }
    public class UIManager
    {
        public void DisplayPlayerResources(Player player1, Player player2)
        {
            Console.WriteLine("=== Player Resources ===");
            Console.WriteLine($"Player 1: Mana: {player1.Mana}, Rage: {player1.Rage}, Health: {player1.Health}");
            Console.WriteLine($"Player 2: Mana: {player2.Mana}, Rage: {player2.Rage}, Health: {player2.Health}");
        }

        public void ShowResourceEffect(int x, int y, ResourceManager.ResourceType type)
        {
            Console.WriteLine($"Player collected {type} at ({x}, {y})!");
        }

        public void DisplayBoard(BoardManager boardManager)
        {
            Console.WriteLine("=== Game Board ===");
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    switch (boardManager.board[i, j])
                    {
                        case BoardManager.CellState.Empty:
                            Console.Write(". ");
                            break;
                        case BoardManager.CellState.Player1:
                            Console.Write("X ");
                            break;
                        case BoardManager.CellState.Player2:
                            Console.Write("O ");
                            break;
                        case BoardManager.CellState.Resource:
                            Console.Write("R ");
                            break;
                    }
                }
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Khởi tạo GameManager
            GameManager gameManager = new GameManager();

            // Bắt đầu trò chơi
            gameManager.StartGame();
        }
    }
}
