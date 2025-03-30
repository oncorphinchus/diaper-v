using System;
using System.Collections.Generic;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Represents a PowerShell command parameter
    /// </summary>
    public class CommandParameter
    {
        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the parameter value
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the CommandParameter class
        /// </summary>
        public CommandParameter()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the CommandParameter class with the specified name and value
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public CommandParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
    
    /// <summary>
    /// Represents a collection of PowerShell command parameters
    /// </summary>
    public class CommandParameterCollection : List<CommandParameter>
    {
        /// <summary>
        /// Initializes a new instance of the CommandParameterCollection class
        /// </summary>
        public CommandParameterCollection()
            : base()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the CommandParameterCollection class with the specified capacity
        /// </summary>
        /// <param name="capacity">The initial capacity of the collection</param>
        public CommandParameterCollection(int capacity)
            : base(capacity)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the CommandParameterCollection class with the specified parameters
        /// </summary>
        /// <param name="parameters">The parameters to include in the collection</param>
        public CommandParameterCollection(IEnumerable<CommandParameter> parameters)
            : base(parameters)
        {
        }
        
        /// <summary>
        /// Adds a parameter with the specified name and value to the collection
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public void Add(string name, object value)
        {
            Add(new CommandParameter(name, value));
        }
        
        /// <summary>
        /// Gets a parameter by name
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <returns>The parameter, or null if not found</returns>
        public CommandParameter GetByName(string name)
        {
            return Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Gets the value of a parameter by name
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <returns>The parameter value, or null if not found</returns>
        public object GetValueByName(string name)
        {
            var parameter = GetByName(name);
            return parameter?.Value;
        }
        
        /// <summary>
        /// Gets the value of a parameter by name and converts it to the specified type
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="name">The parameter name</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found</param>
        /// <returns>The parameter value, or the default value if not found or cannot be converted</returns>
        public T GetValueByName<T>(string name, T defaultValue = default)
        {
            var value = GetValueByName(name);
            
            if (value == null)
            {
                return defaultValue;
            }
            
            if (value is T typedValue)
            {
                return typedValue;
            }
            
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
} 