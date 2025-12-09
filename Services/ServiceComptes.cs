public class ServiceComptes
{
    private readonly List<Compte> _comptes = new();

    public ServiceComptes()
    {
        // Insertion de fausses données au lancement (5 comptes, 15 transactions chacun)
        for (int i = 1; i <= 5; i++)
        {
            var compte = new Compte
            {
                Numero = $"C00{i}2345{i}",
                Titulaire = $"Titulaire {i} (Faux)",
                Type = "Épargne",
                Solde = i * 250000m,
                DateCreation = DateTime.Now.AddMonths(-i)
            };

            // 15 transactions fictives
            for (int j = 1; j <= 15; j++)
            {
                var type = j % 2 == 0 ? "Dépôt" : "Retrait";
                var montant = j * 50000m * (type == "Dépôt" ? 1 : -1);
                compte.Transactions.Add(new Transaction
                {
                    Date = DateTime.Now.AddDays(-j),
                    Type = type,
                    Montant = montant,
                    Description = $"Transaction fake {j}",
                    SoldeApres = compte.Solde + montant  // Calcul fictif
                });
                compte.Solde += montant;  // Mise à jour solde fictif
            }

            _comptes.Add(compte);
        }
    }

    public Compte? GetCompteParNumero(string numero)
    {
        return _comptes.FirstOrDefault(c => c.Numero == numero);
    }
}

public class Compte
{
    public string Numero { get; set; } = string.Empty;
    public string Titulaire { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Solde { get; set; }
    public DateTime DateCreation { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
}

public class Transaction
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public decimal SoldeApres { get; set; }
    public string Description { get; set; } = string.Empty;
}