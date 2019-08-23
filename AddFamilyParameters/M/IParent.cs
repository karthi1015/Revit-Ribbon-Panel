namespace AddFamilyParameters.M
{
    using System.Collections.Generic;

    interface IParent<T>
    {
        IEnumerable<T> GetChildren();
    }
}