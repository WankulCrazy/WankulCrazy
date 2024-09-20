import fs, {promises as fsPromises} from 'fs';


async function main() {
    const input = await fsPromises.readFile('wankul_cards.json', 'utf8');
    const parsed = JSON.parse(input);
    const cards = parsed.cards;

    const rarities = [];
    const raritiesName = [];
    for (const [index, card] of cards.entries()) {
        if (card.rarity.id != 1) {
            const rarity = card.rarity;
            if (rarities.findIndex((item) => item.id == rarity.id) != -1) {
                continue;
            }
            rarities.push(rarity);
            raritiesName.push(rarity.name);
        }

    }


    await fsPromises.writeFile('rarities_wankul_cards.json', JSON.stringify(raritiesName, null, 4));
}

main();