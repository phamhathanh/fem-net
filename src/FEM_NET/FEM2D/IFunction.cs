namespace FEM_NET.FEM2D
{
    interface IFunction<in T, out TResult>
    {
        TResult GetValueAt(T input);
    }
}
