using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server.Controllers {
    [ApiController]
    [Route("api/cards")]
    public class CardsController : Controller {
        List<Card> CardsList = Startup.CardsList;
        public CardsController() {

        }

        [HttpGet]
        public async Task<IActionResult> GetCards() {
            CardsList = await LoadCards();
            return Ok(CardsList);
        }
        [HttpPost]
        public async Task<IActionResult> PostCards(Card card) {
            if (card == null) {
                return BadRequest();
            }
            CardsList.Add(card);
            await SaveCards();
            return Ok();
        }
        [HttpPut]
        public async Task<IActionResult> PutCards(Card card) {
            if (card == null) {
                return BadRequest();
            }
            if (!CardsList.Any(x => x.Id == card.Id)) {
                return NotFound();
            }
            var updateCard = CardsList.FirstOrDefault(x => x.Id == card.Id);
            if (updateCard != null){
                updateCard.Title = card.Title;
                updateCard.Body = card.Body;
            }
            await SaveCards();
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCards(int id) {
            Card card = CardsList.FirstOrDefault(x => x.Id == id);
            if (card == null) {
                return NotFound();
            }
            CardsList.Remove(card);
            await SaveCards();
            return Ok();
        }

        private async Task<List<Card>> LoadCards() {
            string cardsString = await System.IO.File.ReadAllTextAsync(@"cards.json");
            return JsonConvert.DeserializeObject<List<Card>>(cardsString);
        }

        private async Task SaveCards() {
            var json = JsonConvert.SerializeObject(CardsList);
            await System.IO.File.WriteAllTextAsync(@"cards.json", json);
        }
    }
}
