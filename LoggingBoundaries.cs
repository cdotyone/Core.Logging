#region Copyright / Comments

// <copyright file="LoggingBoundaries.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>cdoty@polaropposite.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

namespace Civic.Core.Logging
{
	public enum LoggingBoundaries
	{
        Unknown,
		UI,
		ServiceBoundary, 
		DomainLayer,
		DataLayer,
		Database,
		Host
    }
}
