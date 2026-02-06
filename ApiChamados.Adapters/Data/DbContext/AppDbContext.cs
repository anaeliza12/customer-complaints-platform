using ApiChamados.Adapters.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiChamados.Adapters.Data.dbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ReclamacaoDbModel> Reclamacoes { get; set; }
        public DbSet<ClienteDbModel> Clientes { get; set; }
        public DbSet<AnexoDbModel> Anexos { get; set; }
        public DbSet<ReclamacaoCategoriaDbModel> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ReclamacaoDbModel>()
                .HasOne(r => r.Cliente)
                .WithMany(c => c.Reclamacoes)
                .HasForeignKey(r => r.ClienteId); 

            modelBuilder.Entity<ReclamacaoDbModel>()
                .OwnsOne(r => r.PerfilCliente, p =>
                {
                    p.Property(x => x.Produtos).HasColumnType("jsonb");
                });


            modelBuilder.Entity<AnexoDbModel>()
                .HasOne(a => a.Reclamacao)
                .WithMany(r => r.Anexos)
                .HasForeignKey(a => a.ReclamacaoId);

            modelBuilder.Entity<ReclamacaoCategoriaDbModel>()
                .HasOne(c => c.Reclamacao)
                .WithMany(r => r.Categorias)
                .HasForeignKey(c => c.ReclamacaoId);

            modelBuilder.Entity<ReclamacaoDbModel>(entity =>
            {
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}