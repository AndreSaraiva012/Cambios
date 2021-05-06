namespace Cambios.Servicos
{
    using Cambios.Modelos;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    public class ApiService
    {
        public async Task<Response> GetRates(string urlBase,string controller)
        {
            try
            {
                //criou um http para fazer uma ligação externa e digo onde está o endereço base da api
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase);

                //digo onde está o controlador da api
                var response = await client.GetAsync(controller);

                //carrego os resultados em forma de string para a variavel/objeto result
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }

                var rates = JsonConvert.DeserializeObject<List<Rate>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = rates,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }








    }
}
