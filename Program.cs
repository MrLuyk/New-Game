using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    enum Role { Good, Bad, Badder }
    
    class Player
    {
        public string Name { get; set; }
        public Role Role { get; set; }
        public bool KnowsBadTeam { get; set; } = false;
    }

    static void Main(string[] args)
    {
        List<Player> players = InitializePlayers();
        AssignRoles(players);
        NotifyPlayersOfRoles(players);

        string currentPresident = players[0].Name;
        int consecutiveFailedVotes = 0;
        int positivePolicies = 0;
        int negativePolicies = 0;
        List<string> policyDeck = GeneratePolicyDeck();

        while (positivePolicies < 5 && negativePolicies < 6)
        {
            Console.WriteLine($"\n{currentPresident} is the President.");
            Player chancellor = currentPresident == "Human" ? NominateChancellor(players, currentPresident) : AINominateChancellor(players, currentPresident);

            if (chancellor == null)
            {
                Console.WriteLine("Invalid Chancellor nomination. Skipping turn.");
                currentPresident = GetNextPresident(players, currentPresident);
                continue;
            }

            Console.WriteLine($"{currentPresident} nominates {chancellor.Name} as Chancellor.");

            if (VoteForChancellor(players, chancellor))
            {
                Console.WriteLine($"{chancellor.Name} has been elected Chancellor.");

                if (chancellor.Role == Role.Badder && negativePolicies >= 3)
                {
                    Console.WriteLine($"{chancellor.Name} is the Badder, and has been elected Chancellor with at least 3 Negative policies in play. The Bad team wins!");
                    return;
                }

                (positivePolicies, negativePolicies) = PolicyPhase(currentPresident, chancellor.Name, policyDeck, positivePolicies, negativePolicies, players);

                consecutiveFailedVotes = 0;
            }
            else
            {
                Console.WriteLine($"{chancellor.Name} was not elected Chancellor.");
                consecutiveFailedVotes++;
                if (consecutiveFailedVotes >= 3)
                {
                    Console.WriteLine("Three failed votes in a row! Drawing a random policy.");
                    string randomPolicy = policyDeck.First();
                    policyDeck.RemoveAt(0);
                    Console.WriteLine($"Random policy played: {randomPolicy}");
                    if (randomPolicy == "Positive") positivePolicies++;
                    else negativePolicies++;
                    consecutiveFailedVotes = 0;
                }
            }

            Console.WriteLine($"Positive Policies: {positivePolicies}, Negative Policies: {negativePolicies}");
            currentPresident = GetNextPresident(players, currentPresident);
        }

        DeclareWinner(positivePolicies, negativePolicies, players);
    }

    static List<Player> InitializePlayers()
    {
        List<Player> players = new List<Player>();
        players.Add(new Player { Name = "Human" });
        for (int i = 1; i <= 6; i++)
        {
            players.Add(new Player { Name = i.ToString() });
        }
        return players;
    }

    static void AssignRoles(List<Player> players)
    {
        Random random = new Random();
        List<Role> roles = new List<Role> { Role.Badder, Role.Bad, Role.Good, Role.Good, Role.Good, Role.Good, Role.Good };
        roles = roles.OrderBy(_ => random.Next()).ToList();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].Role = roles[i];
            if (roles[i] == Role.Bad || roles[i] == Role.Badder)
            {
                players[i].KnowsBadTeam = true;
            }
        }
    }

    static void NotifyPlayersOfRoles(List<Player> players)
    {
        foreach (var player in players)
        {
            if (player.Name == "Human")
            {
                Console.WriteLine($"Your role is: {player.Role}");
                if (player.Role == Role.Bad || player.Role == Role.Badder)
                {
                    Console.WriteLine("You know who the other Bad players are:");
                    foreach (var badPlayer in players.Where(p => (p.Role == Role.Bad || p.Role == Role.Badder) && p.Name != "Human"))
                    {
                        Console.WriteLine(badPlayer.Name);
                    }
                }
            }
        }
    }

    static Player NominateChancellor(List<Player> players, string presidentName)
    {
        Console.WriteLine("Nominate a Chancellor by typing their number:");
        foreach (var player in players)
        {
            if (player.Name != presidentName)
            {
                Console.WriteLine(player.Name);
            }
        }

        string choice;
        do
        {
            choice = Console.ReadLine();
        } while (!string.IsNullOrWhiteSpace(choice) && !players.Any(p => p.Name == choice && p.Name != presidentName));

        return players.FirstOrDefault(p => p.Name == choice);
    }

    static Player AINominateChancellor(List<Player> players, string presidentName)
    {
        Random random = new Random();
        List<Player> eligiblePlayers = players.Where(p => p.Name != presidentName).ToList();
        return eligiblePlayers[random.Next(eligiblePlayers.Count)];
    }

    static bool VoteForChancellor(List<Player> players, Player chancellor)
    {
        Console.WriteLine("Vote for the Chancellor (Yes/No):");
        int yesVotes = 0;
        foreach (var player in players)
        {
            string vote = player.Name == "Human" ? Console.ReadLine() : new Random().Next(0, 2) == 0 ? "Yes" : "No";
            Console.WriteLine($"{player.Name} votes {vote}.");
            if (vote == "Yes") yesVotes++;
        }
        return yesVotes > players.Count / 2;
    }

    static (int positive, int negative) PolicyPhase(string president, string chancellor, List<string> deck, int positivePolicies, int negativePolicies, List<Player> players)
    {
        Console.WriteLine("Policy Phase!");
        Random random = new Random();

        List<string> presidentChoices = deck.Take(3).ToList();
        deck.RemoveRange(0, 3);

        string discard;
        if (players.First(p => p.Name == president).Role == Role.Good)
        {
            discard = presidentChoices.Contains("Negative") && presidentChoices.Contains("Positive") ? "Negative" : presidentChoices[random.Next(0, 3)];
        }
        else
        {
            discard = presidentChoices[random.Next(0, 3)];
        }

        presidentChoices.Remove(discard);
        Console.WriteLine($"President {president} discards {discard} policy.");

        string play;
        if (players.First(p => p.Name == chancellor).Role == Role.Good)
        {
            play = presidentChoices.Contains("Positive") ? "Positive" : presidentChoices.First();
        }
        else
        {
            play = presidentChoices[random.Next(0, 2)];
        }

        Console.WriteLine($"Chancellor {chancellor} plays {play} policy.");
        if (play == "Positive") positivePolicies++;
        else negativePolicies++;

        return (positivePolicies, negativePolicies);
    }

    static List<string> GeneratePolicyDeck()
    {
        List<string> deck = new List<string>();
        deck.AddRange(Enumerable.Repeat("Positive", 6));
        deck.AddRange(Enumerable.Repeat("Negative", 11));
        return deck.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    static string GetNextPresident(List<Player> players, string currentPresident)
    {
        int index = players.FindIndex(p => p.Name == currentPresident);
        return players[(index + 1) % players.Count].Name;
    }

    static void DeclareWinner(int positivePolicies, int negativePolicies, List<Player> players)
    {
        if (positivePolicies >= 5)
        {
            Console.WriteLine("The Good team wins!");
        }
        else if (negativePolicies >= 6)
        {
            Console.WriteLine("The Bad team wins!");
        }
        else
        {
            Player badder = players.FirstOrDefault(p => p.Role == Role.Badder);
            if (badder != null)
            {
                Console.WriteLine($"The Badder player ({badder.Name}) wins by getting voted in as Chancellor!");
            }
            else
            {
                Console.WriteLine("Unexpected error: No Badder player found.");
            }
        }
    }
}
