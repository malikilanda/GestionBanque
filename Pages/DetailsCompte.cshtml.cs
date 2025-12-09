using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionComptesBancaires.Pages
{
    public class DetailsCompteModel : PageModel
    {
        [BindProperty]
        public string? NumeroCompte { get; set; }   // peut être null au début

        public Compte? Compte { get; set; }         // null tant qu’on n’a pas cherché

        public List<Transaction> TransactionsPage { get; set; } = new(); // toujours initialisé

        public string? TypeFilter { get; set; }     // peut être null ou ""

        public int PageActuelle { get; set; } = 1;
        public int TotalPages { get; set; }

        // Statistiques
        public decimal TotalDepots { get; set; }
        public decimal TotalRetraits { get; set; }
        public int NombreTransactions { get; set; }
        public string DerniereTransaction { get; set; } = "-";

        // ⚙️ Fausse base de données en mémoire
        public static List<Compte> Comptes { get; set; } = GenererComptes();

        public void OnGet(string? numeroCompte, string? typeFilter, int page = 1)
        {
            TypeFilter = typeFilter;
            PageActuelle = page;

            // Si aucun numéro envoyé → on laisse juste le champ input affiché
            if (string.IsNullOrWhiteSpace(numeroCompte))
                return;

            Compte = Comptes.FirstOrDefault(c =>
                c.Numero.Equals(numeroCompte, StringComparison.OrdinalIgnoreCase));

            if (Compte == null)
                return;

            // Liste de base triée par date décroissante
            var trans = Compte.Transactions
                .OrderByDescending(t => t.Date)
                .ToList();

            // Filtre Dépôt / Retrait
            if (!string.IsNullOrWhiteSpace(TypeFilter))
                trans = trans.Where(t => t.Type == TypeFilter).ToList();

            // Pagination
            int pageSize = 5;
            TotalPages = (int)Math.Ceiling((double)trans.Count / pageSize);
            TransactionsPage = trans
                .Skip((PageActuelle - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Statistiques
            TotalDepots = Compte.Transactions
                .Where(t => t.Type == "Dépôt")
                .Sum(t => t.Montant);

            TotalRetraits = -Compte.Transactions
                .Where(t => t.Type == "Retrait")
                .Sum(t => t.Montant);

            NombreTransactions = Compte.Transactions.Count;

            DerniereTransaction = Compte.Transactions
                .OrderByDescending(t => t.Date)
                .FirstOrDefault()?.Date.ToString("dd/MM/yyyy") ?? "-";
        }

        public IActionResult OnPost()
        {
            // On redirige en GET avec le numéro saisi
            return RedirectToPage(new { numeroCompte = NumeroCompte });
        }

        // ================================
        //  Génération automatique de FAUSSES DONNÉES
        // ================================
        private static List<Compte> GenererComptes()
        {
            var random = new Random();
            var comptes = new List<Compte>();

            for (int i = 1; i <= 5; i++)
            {
                var compte = new Compte
                {
                    Id = i,
                    Numero = $"C00{i}23456",
                    Titulaire = $"Client {i}",
                    Type = (i % 2 == 0) ? "Épargne" : "Espèce",
                    DateCreation = DateTime.Now.AddMonths(-9),
                    Solde = 0,
                    Statut = "Bloqué (30j)",
                    DateDeblocage = DateTime.Now.AddDays(30),
                };

                decimal solde = 0;
                for (int j = 1; j <= 15; j++)
                {
                    bool depot = random.Next(2) == 0;
                    decimal montant = random.Next(50_000, 200_000);
                    solde += depot ? montant : -montant;

                    compte.Transactions.Add(new Transaction
                    {
                        Id = j,
                        Date = DateTime.Now.AddDays(-j * 3),
                        Type = depot ? "Dépôt" : "Retrait",
                        Montant = depot ? montant : -montant,
                        SoldeApres = solde,
                        Description = depot ? "Virement salaire" : "Retrait guichet"
                    });
                }

                compte.Solde = solde;
                comptes.Add(compte);
            }

            return comptes;
        }
    }

    // ================================
    //  Entités
    // ================================
    public class Compte
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Titulaire { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Solde { get; set; }
        public DateTime DateCreation { get; set; }
        public string Statut { get; set; } = string.Empty;
        public DateTime DateDeblocage { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
    }

    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;      // "Dépôt" ou "Retrait"
        public decimal Montant { get; set; }
        public decimal SoldeApres { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
