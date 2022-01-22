using System.Collections.Generic;

public interface IDescendible<T>
{
    /// <summary>
    /// The loss function that gradient descent aims to minimise
    /// The lower value this returns the more 'optimal' the input
    /// </summary>
    float LossFunction(T input);

    /// <summary>
    /// Since we are unable to differentiate the loss functions, we can't mathematically determine
    /// the direction of steepest descent. This means we need to approximate it and we can do this by
    /// returning a list of directions that the input type can be moved in, scaled by our learning rate epsilon.
    /// Using the default provided functions; for a float we use -1, 0, 1 (multiplied by epsilon),
    /// for vector2 we use the 4 cardinal directions and the diagonals.
    /// </summary>
    /// <param name="epsilon">Scales how much we alter input each iteration</param>
    /// <returns></returns>
    List<T> GetDescentDirections(T epsilon);
    T AddInputs(T t1, T t2);
    T ClampInputs(T value, T min, T max);
}
