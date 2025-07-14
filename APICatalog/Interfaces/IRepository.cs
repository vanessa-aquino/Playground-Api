using System.Linq.Expressions;

namespace APICatalog.Interfaces;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();

    // Expression representa a classe de funções lambda, Func de T, que vai me retornar um bool, é um delegate (uma função que pode ser passada como argumento), que representa uma função lambda com base no predicado.
    // Predicate é o critério que será usado no momento da filtragem.
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate); 
    T Create(T entity);
    T Update(T entity);
    T Delete(T entity);
}
