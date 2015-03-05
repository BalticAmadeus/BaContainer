using System;

namespace BalticAmadeus.Container
{
	public interface IContainer: IDisposable
	{
		/// <summary>
		/// Creates or resolves the given type's dependency tree using configuration.
		/// </summary>
		/// <typeparam name="TInterface">Interface or class type that should be resolved.</typeparam>
		/// <returns>Created or resolved instance.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		TInterface Resolve<TInterface>();

		/// <summary>
		/// Creates or resolves the given type's dependency tree using configuration.
		/// Also this method stores the tree to allow to release it later using <see cref="Release{T}"/> method.
		/// </summary>
		/// <param name="type">Interface or class type that should be resolved.</param>
		/// <returns>Created or resolved instance.</returns>
		object Resolve(Type type);

		/// <summary>
		/// Releases the specified parameter and the all dependency tree that was created 
		/// during during the call of <see cref="Resolve{T}"/> method.
		/// </summary>
		/// <typeparam name="TInterface">Interface or class type that should be released.</typeparam>
		/// <param name="instance">Instance to release.</param>
		void Release<TInterface>(TInterface instance);
	}
}