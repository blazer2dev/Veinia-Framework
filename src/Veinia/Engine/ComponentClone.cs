using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace VeiniaFramework
{
	internal static class ComponentClone
	{
		private static readonly MethodInfo memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly ConcurrentDictionary<Type, FieldPlan[]> plans = new ConcurrentDictionary<Type, FieldPlan[]>();

		internal static Component Clone(Component source)
		{
			var plan = GetPlan(source.GetType());

			if (plan.Length == 0) return source.CloneShallow();

			return new CloneContext().CloneComponent(source, plan);
		}

		private static FieldPlan[] GetPlan(Type type) => plans.GetOrAdd(type, BuildPlan);

		private static FieldPlan[] BuildPlan(Type type)
		{
			var result = new List<FieldPlan>();

			for (var current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				var fields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				for (int i = 0; i < fields.Length; i++)
				{
					var field = fields[i];
					var clone = field.GetCustomAttribute<CloneOnInstantiateAttribute>() != null;
					var reset = field.GetCustomAttribute<ResetOnInstantiateAttribute>() != null;

					if (!clone && !reset) continue;
					if (clone && reset) throw new InvalidOperationException($"Field {type.FullName}.{field.Name} cannot use both clone attributes.");
					if (field.IsStatic || field.IsInitOnly) throw new InvalidOperationException($"Field {type.FullName}.{field.Name} must be writable to use a clone attribute.");

					result.Add(new FieldPlan(field, clone));
				}
			}

			return result.ToArray();
		}

		private readonly struct FieldPlan
		{
			internal readonly FieldInfo field;
			internal readonly bool clone;

			internal FieldPlan(FieldInfo field, bool clone)
			{
				this.field = field;
				this.clone = clone;
			}
		}

		private sealed class CloneContext
		{
			private readonly Dictionary<object, object> clones = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

			internal Component CloneComponent(Component source, FieldPlan[] plan)
			{
				var clone = source.CloneShallow();
				clones[source] = clone;
				Apply(source, clone, plan);
				return clone;
			}

			private object CloneObject(object source)
			{
				if (clones.TryGetValue(source, out var existing)) return existing;

				var clone = memberwiseClone.Invoke(source, null);
				clones[source] = clone;
				Apply(source, clone, GetPlan(source.GetType()));
				return clone;
			}

			private void Apply(object source, object clone, FieldPlan[] plan)
			{
				for (int i = 0; i < plan.Length; i++)
				{
					var item = plan[i];
					item.field.SetValue(clone, item.clone ? CloneValue(item.field.GetValue(source)) : DefaultValue(item.field.FieldType));
				}
			}

			private object CloneValue(object value)
			{
				if (value == null || value is string || value.GetType().IsValueType)
					return value;

				if (value is Component component)
					return component.Clone();

				if (value is Array array)
				{
					if (array.Rank != 1) throw new NotSupportedException("Only one-dimensional arrays can be cloned automatically.");

					var elementType = array.GetType().GetElementType();
					var clone = Array.CreateInstance(elementType, array.Length);
					clones[value] = clone;

					for (int i = 0; i < array.Length; i++)
						clone.SetValue(CloneValue(array.GetValue(i)), i);

					return clone;
				}

				if (value is IDictionary dictionary)
				{
					var clone = (IDictionary)Activator.CreateInstance(value.GetType());
					clones[value] = clone;

					foreach (DictionaryEntry item in dictionary)
						clone.Add(CloneValue(item.Key), CloneValue(item.Value));

					return clone;
				}

				if (value is IList list)
				{
					var clone = (IList)Activator.CreateInstance(value.GetType());
					clones[value] = clone;

					foreach (var item in list)
						clone.Add(CloneValue(item));

					return clone;
				}

				if (value is ICloneable cloneable)
					return cloneable.Clone();

				return CloneObject(value);
			}

			private static object DefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}
