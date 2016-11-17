namespace _2DFEM
{
    interface IFunction<in T, out TResult>
    {
        TResult GetValueAt(T input);
    }
}
