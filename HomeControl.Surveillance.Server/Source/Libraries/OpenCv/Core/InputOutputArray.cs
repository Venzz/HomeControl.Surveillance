namespace OpenCv
{
    /// <summary>
    /// Proxy datatype for passing Mat's and vector&lt;&gt;'s as input parameters.
    /// Synonym for OutputArray.
    /// </summary>
    public class InputOutputArray: OutputArray
    {
        internal InputOutputArray(Mat mat): base(mat)
        {
        }

        public static implicit operator InputOutputArray(Mat mat)
        {
            return new InputOutputArray(mat);
        }
    }
}
