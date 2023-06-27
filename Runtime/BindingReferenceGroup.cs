﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace Audune.Utils.InputSystem
{
  // Class that defines a group of references to an input binding
  // This is a one-to-many mapping from an input action to multiple input bindings, effectively grouping bindings based on their action and optional part of composite name
  public sealed class BindingReferenceGroup : IEnumerable<BindingReference>, IEquatable<BindingReferenceGroup>
  {
    // Binding reference group values
    public readonly InputAction action;
    public readonly InputControlScheme controlScheme;
    public readonly IReadOnlyList<BindingReference> bindings;
    public readonly string partOfCompositeName;


    // Return the name of the binding reference group
    public string Name {
      get {
        var actionName = action.actionMap != null && !string.IsNullOrEmpty(action.actionMap.name) ? $"{action.actionMap.name}/{action.name}" : action.name;
        return !string.IsNullOrEmpty(partOfCompositeName) ? $"{actionName}/{partOfCompositeName}" : actionName;
      }
    }


    // Constructor
    public BindingReferenceGroup(InputAction action, InputControlScheme controlScheme, IEnumerable<BindingReference> bindings, string partOfCompositeName)
    {
      this.action = action;
      this.controlScheme = controlScheme;
      this.bindings = bindings.ToList();
      this.partOfCompositeName = partOfCompositeName;
    }
    
    // Return the string representation of the binding reference group
    public override string ToString()
    {
      return $"{Name} bound to {string.Join(", ", bindings.Select(binding => binding.binding.ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice)))}";
    }


    #region Rebinding operations
    // Apply a binding override on a binding that matches the specified predicate in the binding reference group
    public void ApplyBindingOverride(Func<BindingReference, bool> bindingPredicate, InputBinding bindingOverride)
    {
      var binding = bindings.FirstOrDefault(bindingPredicate);
      if (binding == null)
        throw new ArgumentException("Could not find a binding that matches the predicate", nameof(binding));

      binding.ApplyBindingOverride(bindingOverride);
    }

    // Apply a binding override on a binding with the specified binding index in the binding reference group
    public void ApplyBindingOverride(int bindingIndex, InputBinding bindingOverride)
    {
      ApplyBindingOverride(binding => binding.bindingIndex == bindingIndex, bindingOverride);
    }

    // Remove a binding override on a binding that matches the specified predicate in the binding reference group
    public void RemoveBindingOverride(Func<BindingReference, bool> bindingPredicate)
    {
      var binding = bindings.FirstOrDefault(bindingPredicate);
      if (binding == null)
        throw new ArgumentException("Could not find a binding that matches the predicate", nameof(binding));

      binding.RemoveBindingOverride();
    }

    // Remove a binding override on a binding with the specified binding index in the binding reference group
    public void RemoveBindingOverride(int bindingIndex)
    {
      RemoveBindingOverride(binding => binding.bindingIndex == bindingIndex);
    }

    // Perform interactive rebinding on a binding that matches the specified predicate in the binding reference group
    public InputActionRebindingExtensions.RebindingOperation PerformInteractiveRebinding(Func<BindingReference, bool> bindingPredicate)
    {
      var binding = bindings.FirstOrDefault(bindingPredicate);
      if (binding == null)
        throw new ArgumentException("Could not find a binding that matches the predicate", nameof(binding));

      return binding.PerformInteractiveRebinding();
    }

    // Perform interactive rebinding on a binding with the specified binding index in the binding reference group
    public InputActionRebindingExtensions.RebindingOperation PerformInteractiveRebinding(int bindingIndex)
    {
      return PerformInteractiveRebinding(binding => binding.bindingIndex == bindingIndex);
    }
    #endregion

    #region Enumerable implementation
    // Return a generic enumerator of the grouping
    public IEnumerator<BindingReference> GetEnumerator()
    {
      return bindings.GetEnumerator();
    }

    // Return an enumerator of the grouping
    IEnumerator IEnumerable.GetEnumerator()
    {
      return bindings.GetEnumerator();
    }
    #endregion

    #region Equatable implementation
    // Return if the binding reference group equals another object
    public override bool Equals(object obj)
    {
      return Equals(obj as BindingReferenceGroup);
    }

    // Return if the binding reference group equals another binding reference group
    public bool Equals(BindingReferenceGroup other)
    {
      return other is not null &&
        EqualityComparer<InputAction>.Default.Equals(action, other.action) &&
        controlScheme.Equals(other.controlScheme) &&
        partOfCompositeName == other.partOfCompositeName &&
        EqualityComparer<IReadOnlyList<BindingReference>>.Default.Equals(bindings, other.bindings);
    }

    // Return the hash code of the binding composite
    public override int GetHashCode()
    {
      return HashCode.Combine(action, controlScheme, partOfCompositeName, bindings);
    }
    #endregion

    #region Equality operators
    // Return if the binding reference group equals another binding reference group
    public static bool operator ==(BindingReferenceGroup left, BindingReferenceGroup right)
    {
      return EqualityComparer<BindingReferenceGroup>.Default.Equals(left, right);
    }

    // Return if the binding reference group does not equal another binding reference group
    public static bool operator !=(BindingReferenceGroup left, BindingReferenceGroup right)
    {
      return !(left == right);
    }
    #endregion
  }
}