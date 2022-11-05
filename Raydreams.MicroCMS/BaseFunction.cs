using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raydreams.MicroCMS
{
	/// <summary>Serves as the BASE API function for all other functions in .NET 5</summary>
	public abstract class BaseFunction
	{
		/// <summary>The concrete gateway</summary>
		private ICMSGateway _gate;

		public BaseFunction( ICMSGateway gateway )
		{
			_gate = gateway;
		}

		/// <summary>Get the Gateway</summary>
		/// <remarks>Return a NullGateway if the Gateway is null</remarks>
		protected ICMSGateway Gateway => _gate;

		/// <summary>Just a convenience method to add the Request Headers to the Gateway as we can't intercept them during startup</summary>
		/// <remarks>For now there is no way to intercept the Request during Service creation and pull these headers.</remarks>
		protected void AddHeaders( HttpRequestData req )
		{
			this.Gateway.AddHeaders( req );
		}
	}
}
