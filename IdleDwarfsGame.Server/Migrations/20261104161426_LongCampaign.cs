using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class LongCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "A magma golem leads scouts through the smouldering tunnels.");

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "A skeleton mage has joined the golems in the hall of ash.");

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "The ember drake's camp. Its aura sets everything ablaze.");

            migrationBuilder.InsertData(
                table: "CampaignStages",
                columns: new[] { "Id", "ArenaId", "Description", "EnemyLevel", "Name", "Type" },
                values: new object[,]
                {
                    { 11, 3, "Storm creatures crackle with lightning on the slopes.", 13, "Thundering Steps I", 0 },
                    { 12, 3, "Storm creatures crackle with lightning on the slopes.", 14, "Cloudbreak Ridge I", 0 },
                    { 13, 3, "Storm creatures crackle with lightning on the slopes.", 14, "Spark Hollow I", 0 },
                    { 14, 3, "Storm creatures crackle with lightning on the slopes.", 15, "Rumbling Cliffs I", 0 },
                    { 15, 3, "Storm creatures crackle with lightning on the slopes.", 15, "Static Fields I", 0 },
                    { 16, 3, "Storm creatures crackle with lightning on the slopes.", 15, "Lightning Spire I", 0 },
                    { 17, 3, "Storm creatures crackle with lightning on the slopes.", 15, "Howling Saddle I", 0 },
                    { 18, 3, "Storm creatures crackle with lightning on the slopes.", 15, "Stormwatch I", 0 },
                    { 19, 3, "Storm creatures crackle with lightning on the slopes.", 16, "Skyforge Ruins I", 0 },
                    { 20, 3, "Voltaris rides the storm above the summit.", 16, "Eye of the Storm I", 1 },
                    { 21, 4, "Poison drips from every branch of the marsh.", 16, "Rotting Shallows I", 0 },
                    { 22, 4, "Poison drips from every branch of the marsh.", 16, "Fen of Thorns I", 0 },
                    { 23, 4, "Poison drips from every branch of the marsh.", 16, "Mossy Causeway I", 0 },
                    { 24, 4, "Poison drips from every branch of the marsh.", 16, "Drowned Grove I", 0 },
                    { 25, 4, "Poison drips from every branch of the marsh.", 16, "Toad Hollow I", 0 },
                    { 26, 4, "Poison drips from every branch of the marsh.", 16, "Blackwater Bend I", 0 },
                    { 27, 4, "Poison drips from every branch of the marsh.", 17, "Sporefield I", 0 },
                    { 28, 4, "Poison drips from every branch of the marsh.", 17, "Hanging Roots I", 0 },
                    { 29, 4, "Poison drips from every branch of the marsh.", 17, "Witchlight Bog I", 0 },
                    { 30, 4, "Mirefang waits where the bog runs deepest.", 17, "Heart of the Mire I", 1 },
                    { 31, 5, "The dead of the crypt rise against the intruders.", 17, "Silent Ossuary I", 0 },
                    { 32, 5, "The dead of the crypt rise against the intruders.", 17, "Hall of Bones I", 0 },
                    { 33, 5, "The dead of the crypt rise against the intruders.", 18, "Whispering Vault I", 0 },
                    { 34, 5, "The dead of the crypt rise against the intruders.", 18, "Gravewind Stair I", 0 },
                    { 35, 5, "The dead of the crypt rise against the intruders.", 18, "Chapel of Ash I", 0 },
                    { 36, 5, "The dead of the crypt rise against the intruders.", 19, "Forgotten Catacombs I", 0 },
                    { 37, 5, "The dead of the crypt rise against the intruders.", 19, "Mourners' Gallery I", 0 },
                    { 38, 5, "The dead of the crypt rise against the intruders.", 19, "Sunless Depths I", 0 },
                    { 39, 5, "The dead of the crypt rise against the intruders.", 19, "Sealed Reliquary I", 0 },
                    { 40, 5, "Morvath the Hollow rules the deepest vault.", 19, "Throne of Night I", 1 },
                    { 41, 6, "Crystal guardians shine with blinding light.", 20, "Prism Gate I", 0 },
                    { 42, 6, "Crystal guardians shine with blinding light.", 20, "Shimmering Hall I", 0 },
                    { 43, 6, "Crystal guardians shine with blinding light.", 20, "Glass Gardens I", 0 },
                    { 44, 6, "Crystal guardians shine with blinding light.", 21, "Hall of Mirrors I", 0 },
                    { 45, 6, "Crystal guardians shine with blinding light.", 21, "Choir of Light I", 0 },
                    { 46, 6, "Crystal guardians shine with blinding light.", 21, "Radiant Nave I", 0 },
                    { 47, 6, "Crystal guardians shine with blinding light.", 21, "Sunstone Bridge I", 0 },
                    { 48, 6, "Crystal guardians shine with blinding light.", 21, "Gleaming Spire I", 0 },
                    { 49, 6, "Crystal guardians shine with blinding light.", 22, "Starlit Archive I", 0 },
                    { 50, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 22, "Crown of Dawn I", 1 },
                    { 51, 7, "Garnet cultists thirst for fresh blood.", 22, "Crimson Steps I", 0 },
                    { 52, 7, "Garnet cultists thirst for fresh blood.", 23, "Bleeding Stones I", 0 },
                    { 53, 7, "Garnet cultists thirst for fresh blood.", 24, "Hall of Offerings I", 0 },
                    { 54, 7, "Garnet cultists thirst for fresh blood.", 24, "Red Pools I", 0 },
                    { 55, 7, "Garnet cultists thirst for fresh blood.", 24, "Thorned Pillars I", 0 },
                    { 56, 7, "Garnet cultists thirst for fresh blood.", 25, "Cutter's Alley I", 0 },
                    { 57, 7, "Garnet cultists thirst for fresh blood.", 26, "Vein Tunnels I", 0 },
                    { 58, 7, "Garnet cultists thirst for fresh blood.", 26, "Garnet Court I", 0 },
                    { 59, 7, "Garnet cultists thirst for fresh blood.", 26, "Sacrificial Ring I", 0 },
                    { 60, 7, "Vargul the Bloodbound stands over the great altar.", 27, "The Great Altar I", 1 },
                    { 61, 1, "Fire tribes guard the forges deeper under the mountain.", 27, "Ashen Road I", 0 },
                    { 62, 1, "Fire tribes guard the forges deeper under the mountain.", 28, "Magma Falls I", 0 },
                    { 63, 1, "Fire tribes guard the forges deeper under the mountain.", 28, "Burning Bridge I", 0 },
                    { 64, 1, "Fire tribes guard the forges deeper under the mountain.", 29, "Slag Pits I", 0 },
                    { 65, 1, "Fire tribes guard the forges deeper under the mountain.", 29, "Obsidian Hall I", 0 },
                    { 66, 1, "Fire tribes guard the forges deeper under the mountain.", 30, "Ember Wastes I", 0 },
                    { 67, 1, "Fire tribes guard the forges deeper under the mountain.", 30, "Firebrand Camp I", 0 },
                    { 68, 1, "Fire tribes guard the forges deeper under the mountain.", 30, "Molten Vault I", 0 },
                    { 69, 1, "Fire tribes guard the forges deeper under the mountain.", 31, "Smelter's Hold I", 0 },
                    { 70, 1, "Ignar the Ashborn rises from the caldera.", 31, "Caldera Throne I", 1 },
                    { 71, 2, "Frozen hunters stalk the white wastes.", 32, "Snowblind Path I", 0 },
                    { 72, 2, "Frozen hunters stalk the white wastes.", 32, "Frostfang Gorge I", 0 },
                    { 73, 2, "Frozen hunters stalk the white wastes.", 33, "Frozen Lake I", 0 },
                    { 74, 2, "Frozen hunters stalk the white wastes.", 33, "Icicle Cave I", 0 },
                    { 75, 2, "Frozen hunters stalk the white wastes.", 33, "Hailstone Ridge I", 0 },
                    { 76, 2, "Frozen hunters stalk the white wastes.", 34, "Glacier Heart I", 0 },
                    { 77, 2, "Frozen hunters stalk the white wastes.", 34, "Whiteout Plains I", 0 },
                    { 78, 2, "Frozen hunters stalk the white wastes.", 35, "Rimehold Keep I", 0 },
                    { 79, 2, "Frozen hunters stalk the white wastes.", 35, "Avalanche Pass I", 0 },
                    { 80, 2, "Kryssa the Winter Drake guards the peak.", 35, "Peak of Winter I", 1 },
                    { 81, 3, "Storm creatures crackle with lightning on the slopes.", 36, "Thundering Steps II", 0 },
                    { 82, 3, "Storm creatures crackle with lightning on the slopes.", 36, "Cloudbreak Ridge II", 0 },
                    { 83, 3, "Storm creatures crackle with lightning on the slopes.", 37, "Spark Hollow II", 0 },
                    { 84, 3, "Storm creatures crackle with lightning on the slopes.", 37, "Rumbling Cliffs II", 0 },
                    { 85, 3, "Storm creatures crackle with lightning on the slopes.", 38, "Static Fields II", 0 },
                    { 86, 3, "Storm creatures crackle with lightning on the slopes.", 38, "Lightning Spire II", 0 },
                    { 87, 3, "Storm creatures crackle with lightning on the slopes.", 38, "Howling Saddle II", 0 },
                    { 88, 3, "Storm creatures crackle with lightning on the slopes.", 39, "Stormwatch II", 0 },
                    { 89, 3, "Storm creatures crackle with lightning on the slopes.", 39, "Skyforge Ruins II", 0 },
                    { 90, 3, "Voltaris rides the storm above the summit.", 40, "Eye of the Storm II", 1 },
                    { 91, 4, "Poison drips from every branch of the marsh.", 40, "Rotting Shallows II", 0 },
                    { 92, 4, "Poison drips from every branch of the marsh.", 41, "Fen of Thorns II", 0 },
                    { 93, 4, "Poison drips from every branch of the marsh.", 41, "Mossy Causeway II", 0 },
                    { 94, 4, "Poison drips from every branch of the marsh.", 41, "Drowned Grove II", 0 },
                    { 95, 4, "Poison drips from every branch of the marsh.", 42, "Toad Hollow II", 0 },
                    { 96, 4, "Poison drips from every branch of the marsh.", 42, "Blackwater Bend II", 0 },
                    { 97, 4, "Poison drips from every branch of the marsh.", 43, "Sporefield II", 0 },
                    { 98, 4, "Poison drips from every branch of the marsh.", 43, "Hanging Roots II", 0 },
                    { 99, 4, "Poison drips from every branch of the marsh.", 44, "Witchlight Bog II", 0 },
                    { 100, 4, "Mirefang waits where the bog runs deepest.", 44, "Heart of the Mire II", 1 },
                    { 101, 5, "The dead of the crypt rise against the intruders.", 44, "Silent Ossuary II", 0 },
                    { 102, 5, "The dead of the crypt rise against the intruders.", 44, "Hall of Bones II", 0 },
                    { 103, 5, "The dead of the crypt rise against the intruders.", 45, "Whispering Vault II", 0 },
                    { 104, 5, "The dead of the crypt rise against the intruders.", 45, "Gravewind Stair II", 0 },
                    { 105, 5, "The dead of the crypt rise against the intruders.", 45, "Chapel of Ash II", 0 },
                    { 106, 5, "The dead of the crypt rise against the intruders.", 45, "Forgotten Catacombs II", 0 },
                    { 107, 5, "The dead of the crypt rise against the intruders.", 45, "Mourners' Gallery II", 0 },
                    { 108, 5, "The dead of the crypt rise against the intruders.", 46, "Sunless Depths II", 0 },
                    { 109, 5, "The dead of the crypt rise against the intruders.", 46, "Sealed Reliquary II", 0 },
                    { 110, 5, "Morvath the Hollow rules the deepest vault.", 46, "Throne of Night II", 1 },
                    { 111, 6, "Crystal guardians shine with blinding light.", 46, "Prism Gate II", 0 },
                    { 112, 6, "Crystal guardians shine with blinding light.", 46, "Shimmering Hall II", 0 },
                    { 113, 6, "Crystal guardians shine with blinding light.", 47, "Glass Gardens II", 0 },
                    { 114, 6, "Crystal guardians shine with blinding light.", 47, "Hall of Mirrors II", 0 },
                    { 115, 6, "Crystal guardians shine with blinding light.", 47, "Choir of Light II", 0 },
                    { 116, 6, "Crystal guardians shine with blinding light.", 47, "Radiant Nave II", 0 },
                    { 117, 6, "Crystal guardians shine with blinding light.", 47, "Sunstone Bridge II", 0 },
                    { 118, 6, "Crystal guardians shine with blinding light.", 48, "Gleaming Spire II", 0 },
                    { 119, 6, "Crystal guardians shine with blinding light.", 48, "Starlit Archive II", 0 },
                    { 120, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 48, "Crown of Dawn II", 1 },
                    { 121, 7, "Garnet cultists thirst for fresh blood.", 48, "Crimson Steps II", 0 },
                    { 122, 7, "Garnet cultists thirst for fresh blood.", 48, "Bleeding Stones II", 0 },
                    { 123, 7, "Garnet cultists thirst for fresh blood.", 49, "Hall of Offerings II", 0 },
                    { 124, 7, "Garnet cultists thirst for fresh blood.", 49, "Red Pools II", 0 },
                    { 125, 7, "Garnet cultists thirst for fresh blood.", 49, "Thorned Pillars II", 0 },
                    { 126, 7, "Garnet cultists thirst for fresh blood.", 49, "Cutter's Alley II", 0 },
                    { 127, 7, "Garnet cultists thirst for fresh blood.", 49, "Vein Tunnels II", 0 },
                    { 128, 7, "Garnet cultists thirst for fresh blood.", 50, "Garnet Court II", 0 },
                    { 129, 7, "Garnet cultists thirst for fresh blood.", 50, "Sacrificial Ring II", 0 },
                    { 130, 7, "Vargul the Bloodbound stands over the great altar.", 50, "The Great Altar II", 1 },
                    { 131, 1, "Fire tribes guard the forges deeper under the mountain.", 50, "Ashen Road II", 0 },
                    { 132, 1, "Fire tribes guard the forges deeper under the mountain.", 50, "Magma Falls II", 0 },
                    { 133, 1, "Fire tribes guard the forges deeper under the mountain.", 51, "Burning Bridge II", 0 },
                    { 134, 1, "Fire tribes guard the forges deeper under the mountain.", 51, "Slag Pits II", 0 },
                    { 135, 1, "Fire tribes guard the forges deeper under the mountain.", 51, "Obsidian Hall II", 0 },
                    { 136, 1, "Fire tribes guard the forges deeper under the mountain.", 51, "Ember Wastes II", 0 },
                    { 137, 1, "Fire tribes guard the forges deeper under the mountain.", 51, "Firebrand Camp II", 0 },
                    { 138, 1, "Fire tribes guard the forges deeper under the mountain.", 52, "Molten Vault II", 0 },
                    { 139, 1, "Fire tribes guard the forges deeper under the mountain.", 52, "Smelter's Hold II", 0 },
                    { 140, 1, "Ignar the Ashborn rises from the caldera.", 52, "Caldera Throne II", 1 },
                    { 141, 2, "Frozen hunters stalk the white wastes.", 52, "Snowblind Path II", 0 },
                    { 142, 2, "Frozen hunters stalk the white wastes.", 52, "Frostfang Gorge II", 0 },
                    { 143, 2, "Frozen hunters stalk the white wastes.", 53, "Frozen Lake II", 0 },
                    { 144, 2, "Frozen hunters stalk the white wastes.", 53, "Icicle Cave II", 0 },
                    { 145, 2, "Frozen hunters stalk the white wastes.", 53, "Hailstone Ridge II", 0 },
                    { 146, 2, "Frozen hunters stalk the white wastes.", 53, "Glacier Heart II", 0 },
                    { 147, 2, "Frozen hunters stalk the white wastes.", 53, "Whiteout Plains II", 0 },
                    { 148, 2, "Frozen hunters stalk the white wastes.", 54, "Rimehold Keep II", 0 },
                    { 149, 2, "Frozen hunters stalk the white wastes.", 54, "Avalanche Pass II", 0 },
                    { 150, 2, "Kryssa the Winter Drake guards the peak.", 54, "Peak of Winter II", 1 },
                    { 151, 3, "Storm creatures crackle with lightning on the slopes.", 54, "Thundering Steps III", 0 },
                    { 152, 3, "Storm creatures crackle with lightning on the slopes.", 55, "Cloudbreak Ridge III", 0 },
                    { 153, 3, "Storm creatures crackle with lightning on the slopes.", 55, "Spark Hollow III", 0 },
                    { 154, 3, "Storm creatures crackle with lightning on the slopes.", 55, "Rumbling Cliffs III", 0 },
                    { 155, 3, "Storm creatures crackle with lightning on the slopes.", 55, "Static Fields III", 0 },
                    { 156, 3, "Storm creatures crackle with lightning on the slopes.", 56, "Lightning Spire III", 0 },
                    { 157, 3, "Storm creatures crackle with lightning on the slopes.", 56, "Howling Saddle III", 0 },
                    { 158, 3, "Storm creatures crackle with lightning on the slopes.", 56, "Stormwatch III", 0 },
                    { 159, 3, "Storm creatures crackle with lightning on the slopes.", 56, "Skyforge Ruins III", 0 },
                    { 160, 3, "Voltaris rides the storm above the summit.", 57, "Eye of the Storm III", 1 },
                    { 161, 4, "Poison drips from every branch of the marsh.", 57, "Rotting Shallows III", 0 },
                    { 162, 4, "Poison drips from every branch of the marsh.", 57, "Fen of Thorns III", 0 },
                    { 163, 4, "Poison drips from every branch of the marsh.", 57, "Mossy Causeway III", 0 },
                    { 164, 4, "Poison drips from every branch of the marsh.", 58, "Drowned Grove III", 0 },
                    { 165, 4, "Poison drips from every branch of the marsh.", 58, "Toad Hollow III", 0 },
                    { 166, 4, "Poison drips from every branch of the marsh.", 58, "Blackwater Bend III", 0 },
                    { 167, 4, "Poison drips from every branch of the marsh.", 58, "Sporefield III", 0 },
                    { 168, 4, "Poison drips from every branch of the marsh.", 58, "Hanging Roots III", 0 },
                    { 169, 4, "Poison drips from every branch of the marsh.", 59, "Witchlight Bog III", 0 },
                    { 170, 4, "Mirefang waits where the bog runs deepest.", 59, "Heart of the Mire III", 1 },
                    { 171, 5, "The dead of the crypt rise against the intruders.", 59, "Silent Ossuary III", 0 },
                    { 172, 5, "The dead of the crypt rise against the intruders.", 59, "Hall of Bones III", 0 },
                    { 173, 5, "The dead of the crypt rise against the intruders.", 59, "Whispering Vault III", 0 },
                    { 174, 5, "The dead of the crypt rise against the intruders.", 59, "Gravewind Stair III", 0 },
                    { 175, 5, "The dead of the crypt rise against the intruders.", 59, "Chapel of Ash III", 0 },
                    { 176, 5, "The dead of the crypt rise against the intruders.", 60, "Forgotten Catacombs III", 0 },
                    { 177, 5, "The dead of the crypt rise against the intruders.", 60, "Mourners' Gallery III", 0 },
                    { 178, 5, "The dead of the crypt rise against the intruders.", 60, "Sunless Depths III", 0 },
                    { 179, 5, "The dead of the crypt rise against the intruders.", 60, "Sealed Reliquary III", 0 },
                    { 180, 5, "Morvath the Hollow rules the deepest vault.", 60, "Throne of Night III", 1 },
                    { 181, 6, "Crystal guardians shine with blinding light.", 60, "Prism Gate III", 0 },
                    { 182, 6, "Crystal guardians shine with blinding light.", 60, "Shimmering Hall III", 0 },
                    { 183, 6, "Crystal guardians shine with blinding light.", 61, "Glass Gardens III", 0 },
                    { 184, 6, "Crystal guardians shine with blinding light.", 61, "Hall of Mirrors III", 0 },
                    { 185, 6, "Crystal guardians shine with blinding light.", 61, "Choir of Light III", 0 },
                    { 186, 6, "Crystal guardians shine with blinding light.", 61, "Radiant Nave III", 0 },
                    { 187, 6, "Crystal guardians shine with blinding light.", 61, "Sunstone Bridge III", 0 },
                    { 188, 6, "Crystal guardians shine with blinding light.", 61, "Gleaming Spire III", 0 },
                    { 189, 6, "Crystal guardians shine with blinding light.", 61, "Starlit Archive III", 0 },
                    { 190, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 62, "Crown of Dawn III", 1 },
                    { 191, 7, "Garnet cultists thirst for fresh blood.", 62, "Crimson Steps III", 0 },
                    { 192, 7, "Garnet cultists thirst for fresh blood.", 62, "Bleeding Stones III", 0 },
                    { 193, 7, "Garnet cultists thirst for fresh blood.", 62, "Hall of Offerings III", 0 },
                    { 194, 7, "Garnet cultists thirst for fresh blood.", 62, "Red Pools III", 0 },
                    { 195, 7, "Garnet cultists thirst for fresh blood.", 62, "Thorned Pillars III", 0 },
                    { 196, 7, "Garnet cultists thirst for fresh blood.", 62, "Cutter's Alley III", 0 },
                    { 197, 7, "Garnet cultists thirst for fresh blood.", 63, "Vein Tunnels III", 0 },
                    { 198, 7, "Garnet cultists thirst for fresh blood.", 63, "Garnet Court III", 0 },
                    { 199, 7, "Garnet cultists thirst for fresh blood.", 63, "Sacrificial Ring III", 0 },
                    { 200, 7, "Vargul the Bloodbound stands over the great altar.", 63, "The Great Altar III", 1 },
                    { 201, 1, "Fire tribes guard the forges deeper under the mountain.", 63, "Ashen Road III", 0 },
                    { 202, 1, "Fire tribes guard the forges deeper under the mountain.", 63, "Magma Falls III", 0 },
                    { 203, 1, "Fire tribes guard the forges deeper under the mountain.", 63, "Burning Bridge III", 0 },
                    { 204, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Slag Pits III", 0 },
                    { 205, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Obsidian Hall III", 0 },
                    { 206, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Ember Wastes III", 0 },
                    { 207, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Firebrand Camp III", 0 },
                    { 208, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Molten Vault III", 0 },
                    { 209, 1, "Fire tribes guard the forges deeper under the mountain.", 64, "Smelter's Hold III", 0 },
                    { 210, 1, "Ignar the Ashborn rises from the caldera.", 64, "Caldera Throne III", 1 },
                    { 211, 2, "Frozen hunters stalk the white wastes.", 64, "Snowblind Path III", 0 },
                    { 212, 2, "Frozen hunters stalk the white wastes.", 65, "Frostfang Gorge III", 0 },
                    { 213, 2, "Frozen hunters stalk the white wastes.", 65, "Frozen Lake III", 0 },
                    { 214, 2, "Frozen hunters stalk the white wastes.", 65, "Icicle Cave III", 0 },
                    { 215, 2, "Frozen hunters stalk the white wastes.", 65, "Hailstone Ridge III", 0 },
                    { 216, 2, "Frozen hunters stalk the white wastes.", 65, "Glacier Heart III", 0 },
                    { 217, 2, "Frozen hunters stalk the white wastes.", 65, "Whiteout Plains III", 0 },
                    { 218, 2, "Frozen hunters stalk the white wastes.", 65, "Rimehold Keep III", 0 },
                    { 219, 2, "Frozen hunters stalk the white wastes.", 66, "Avalanche Pass III", 0 },
                    { 220, 2, "Kryssa the Winter Drake guards the peak.", 66, "Peak of Winter III", 1 },
                    { 221, 3, "Storm creatures crackle with lightning on the slopes.", 66, "Thundering Steps IV", 0 },
                    { 222, 3, "Storm creatures crackle with lightning on the slopes.", 66, "Cloudbreak Ridge IV", 0 },
                    { 223, 3, "Storm creatures crackle with lightning on the slopes.", 66, "Spark Hollow IV", 0 },
                    { 224, 3, "Storm creatures crackle with lightning on the slopes.", 66, "Rumbling Cliffs IV", 0 },
                    { 225, 3, "Storm creatures crackle with lightning on the slopes.", 66, "Static Fields IV", 0 },
                    { 226, 3, "Storm creatures crackle with lightning on the slopes.", 67, "Lightning Spire IV", 0 },
                    { 227, 3, "Storm creatures crackle with lightning on the slopes.", 67, "Howling Saddle IV", 0 },
                    { 228, 3, "Storm creatures crackle with lightning on the slopes.", 67, "Stormwatch IV", 0 },
                    { 229, 3, "Storm creatures crackle with lightning on the slopes.", 67, "Skyforge Ruins IV", 0 },
                    { 230, 3, "Voltaris rides the storm above the summit.", 67, "Eye of the Storm IV", 1 },
                    { 231, 4, "Poison drips from every branch of the marsh.", 67, "Rotting Shallows IV", 0 },
                    { 232, 4, "Poison drips from every branch of the marsh.", 67, "Fen of Thorns IV", 0 },
                    { 233, 4, "Poison drips from every branch of the marsh.", 68, "Mossy Causeway IV", 0 },
                    { 234, 4, "Poison drips from every branch of the marsh.", 68, "Drowned Grove IV", 0 },
                    { 235, 4, "Poison drips from every branch of the marsh.", 68, "Toad Hollow IV", 0 },
                    { 236, 4, "Poison drips from every branch of the marsh.", 68, "Blackwater Bend IV", 0 },
                    { 237, 4, "Poison drips from every branch of the marsh.", 68, "Sporefield IV", 0 },
                    { 238, 4, "Poison drips from every branch of the marsh.", 68, "Hanging Roots IV", 0 },
                    { 239, 4, "Poison drips from every branch of the marsh.", 68, "Witchlight Bog IV", 0 },
                    { 240, 4, "Mirefang waits where the bog runs deepest.", 69, "Heart of the Mire IV", 1 },
                    { 241, 5, "The dead of the crypt rise against the intruders.", 69, "Silent Ossuary IV", 0 },
                    { 242, 5, "The dead of the crypt rise against the intruders.", 69, "Hall of Bones IV", 0 },
                    { 243, 5, "The dead of the crypt rise against the intruders.", 69, "Whispering Vault IV", 0 },
                    { 244, 5, "The dead of the crypt rise against the intruders.", 69, "Gravewind Stair IV", 0 },
                    { 245, 5, "The dead of the crypt rise against the intruders.", 69, "Chapel of Ash IV", 0 },
                    { 246, 5, "The dead of the crypt rise against the intruders.", 69, "Forgotten Catacombs IV", 0 },
                    { 247, 5, "The dead of the crypt rise against the intruders.", 70, "Mourners' Gallery IV", 0 },
                    { 248, 5, "The dead of the crypt rise against the intruders.", 70, "Sunless Depths IV", 0 },
                    { 249, 5, "The dead of the crypt rise against the intruders.", 70, "Sealed Reliquary IV", 0 },
                    { 250, 5, "Morvath the Hollow rules the deepest vault.", 70, "Throne of Night IV", 1 },
                    { 251, 6, "Crystal guardians shine with blinding light.", 70, "Prism Gate IV", 0 },
                    { 252, 6, "Crystal guardians shine with blinding light.", 70, "Shimmering Hall IV", 0 },
                    { 253, 6, "Crystal guardians shine with blinding light.", 70, "Glass Gardens IV", 0 },
                    { 254, 6, "Crystal guardians shine with blinding light.", 70, "Hall of Mirrors IV", 0 },
                    { 255, 6, "Crystal guardians shine with blinding light.", 71, "Choir of Light IV", 0 },
                    { 256, 6, "Crystal guardians shine with blinding light.", 71, "Radiant Nave IV", 0 },
                    { 257, 6, "Crystal guardians shine with blinding light.", 71, "Sunstone Bridge IV", 0 },
                    { 258, 6, "Crystal guardians shine with blinding light.", 71, "Gleaming Spire IV", 0 },
                    { 259, 6, "Crystal guardians shine with blinding light.", 71, "Starlit Archive IV", 0 },
                    { 260, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 71, "Crown of Dawn IV", 1 },
                    { 261, 7, "Garnet cultists thirst for fresh blood.", 71, "Crimson Steps IV", 0 },
                    { 262, 7, "Garnet cultists thirst for fresh blood.", 71, "Bleeding Stones IV", 0 },
                    { 263, 7, "Garnet cultists thirst for fresh blood.", 72, "Hall of Offerings IV", 0 },
                    { 264, 7, "Garnet cultists thirst for fresh blood.", 72, "Red Pools IV", 0 },
                    { 265, 7, "Garnet cultists thirst for fresh blood.", 72, "Thorned Pillars IV", 0 },
                    { 266, 7, "Garnet cultists thirst for fresh blood.", 72, "Cutter's Alley IV", 0 },
                    { 267, 7, "Garnet cultists thirst for fresh blood.", 72, "Vein Tunnels IV", 0 },
                    { 268, 7, "Garnet cultists thirst for fresh blood.", 72, "Garnet Court IV", 0 },
                    { 269, 7, "Garnet cultists thirst for fresh blood.", 72, "Sacrificial Ring IV", 0 },
                    { 270, 7, "Vargul the Bloodbound stands over the great altar.", 72, "The Great Altar IV", 1 },
                    { 271, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Ashen Road IV", 0 },
                    { 272, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Magma Falls IV", 0 },
                    { 273, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Burning Bridge IV", 0 },
                    { 274, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Slag Pits IV", 0 },
                    { 275, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Obsidian Hall IV", 0 },
                    { 276, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Ember Wastes IV", 0 },
                    { 277, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Firebrand Camp IV", 0 },
                    { 278, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Molten Vault IV", 0 },
                    { 279, 1, "Fire tribes guard the forges deeper under the mountain.", 73, "Smelter's Hold IV", 0 },
                    { 280, 1, "Ignar the Ashborn rises from the caldera.", 74, "Caldera Throne IV", 1 },
                    { 281, 2, "Frozen hunters stalk the white wastes.", 74, "Snowblind Path IV", 0 },
                    { 282, 2, "Frozen hunters stalk the white wastes.", 74, "Frostfang Gorge IV", 0 },
                    { 283, 2, "Frozen hunters stalk the white wastes.", 74, "Frozen Lake IV", 0 },
                    { 284, 2, "Frozen hunters stalk the white wastes.", 74, "Icicle Cave IV", 0 },
                    { 285, 2, "Frozen hunters stalk the white wastes.", 74, "Hailstone Ridge IV", 0 },
                    { 286, 2, "Frozen hunters stalk the white wastes.", 74, "Glacier Heart IV", 0 },
                    { 287, 2, "Frozen hunters stalk the white wastes.", 74, "Whiteout Plains IV", 0 },
                    { 288, 2, "Frozen hunters stalk the white wastes.", 75, "Rimehold Keep IV", 0 },
                    { 289, 2, "Frozen hunters stalk the white wastes.", 75, "Avalanche Pass IV", 0 },
                    { 290, 2, "Kryssa the Winter Drake guards the peak.", 75, "Peak of Winter IV", 1 },
                    { 291, 3, "Storm creatures crackle with lightning on the slopes.", 75, "Thundering Steps V", 0 },
                    { 292, 3, "Storm creatures crackle with lightning on the slopes.", 75, "Cloudbreak Ridge V", 0 },
                    { 293, 3, "Storm creatures crackle with lightning on the slopes.", 75, "Spark Hollow V", 0 },
                    { 294, 3, "Storm creatures crackle with lightning on the slopes.", 75, "Rumbling Cliffs V", 0 },
                    { 295, 3, "Storm creatures crackle with lightning on the slopes.", 75, "Static Fields V", 0 },
                    { 296, 3, "Storm creatures crackle with lightning on the slopes.", 76, "Lightning Spire V", 0 },
                    { 297, 3, "Storm creatures crackle with lightning on the slopes.", 76, "Howling Saddle V", 0 },
                    { 298, 3, "Storm creatures crackle with lightning on the slopes.", 76, "Stormwatch V", 0 },
                    { 299, 3, "Storm creatures crackle with lightning on the slopes.", 76, "Skyforge Ruins V", 0 },
                    { 300, 3, "Voltaris rides the storm above the summit.", 76, "Eye of the Storm V", 1 },
                    { 301, 4, "Poison drips from every branch of the marsh.", 76, "Rotting Shallows V", 0 },
                    { 302, 4, "Poison drips from every branch of the marsh.", 76, "Fen of Thorns V", 0 },
                    { 303, 4, "Poison drips from every branch of the marsh.", 76, "Mossy Causeway V", 0 },
                    { 304, 4, "Poison drips from every branch of the marsh.", 76, "Drowned Grove V", 0 },
                    { 305, 4, "Poison drips from every branch of the marsh.", 77, "Toad Hollow V", 0 },
                    { 306, 4, "Poison drips from every branch of the marsh.", 77, "Blackwater Bend V", 0 },
                    { 307, 4, "Poison drips from every branch of the marsh.", 77, "Sporefield V", 0 },
                    { 308, 4, "Poison drips from every branch of the marsh.", 77, "Hanging Roots V", 0 },
                    { 309, 4, "Poison drips from every branch of the marsh.", 77, "Witchlight Bog V", 0 },
                    { 310, 4, "Mirefang waits where the bog runs deepest.", 77, "Heart of the Mire V", 1 },
                    { 311, 5, "The dead of the crypt rise against the intruders.", 77, "Silent Ossuary V", 0 },
                    { 312, 5, "The dead of the crypt rise against the intruders.", 77, "Hall of Bones V", 0 },
                    { 313, 5, "The dead of the crypt rise against the intruders.", 78, "Whispering Vault V", 0 },
                    { 314, 5, "The dead of the crypt rise against the intruders.", 78, "Gravewind Stair V", 0 },
                    { 315, 5, "The dead of the crypt rise against the intruders.", 78, "Chapel of Ash V", 0 },
                    { 316, 5, "The dead of the crypt rise against the intruders.", 78, "Forgotten Catacombs V", 0 },
                    { 317, 5, "The dead of the crypt rise against the intruders.", 78, "Mourners' Gallery V", 0 },
                    { 318, 5, "The dead of the crypt rise against the intruders.", 78, "Sunless Depths V", 0 },
                    { 319, 5, "The dead of the crypt rise against the intruders.", 78, "Sealed Reliquary V", 0 },
                    { 320, 5, "Morvath the Hollow rules the deepest vault.", 78, "Throne of Night V", 1 },
                    { 321, 6, "Crystal guardians shine with blinding light.", 79, "Prism Gate V", 0 },
                    { 322, 6, "Crystal guardians shine with blinding light.", 79, "Shimmering Hall V", 0 },
                    { 323, 6, "Crystal guardians shine with blinding light.", 79, "Glass Gardens V", 0 },
                    { 324, 6, "Crystal guardians shine with blinding light.", 79, "Hall of Mirrors V", 0 },
                    { 325, 6, "Crystal guardians shine with blinding light.", 79, "Choir of Light V", 0 },
                    { 326, 6, "Crystal guardians shine with blinding light.", 79, "Radiant Nave V", 0 },
                    { 327, 6, "Crystal guardians shine with blinding light.", 79, "Sunstone Bridge V", 0 },
                    { 328, 6, "Crystal guardians shine with blinding light.", 79, "Gleaming Spire V", 0 },
                    { 329, 6, "Crystal guardians shine with blinding light.", 79, "Starlit Archive V", 0 },
                    { 330, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 80, "Crown of Dawn V", 1 },
                    { 331, 7, "Garnet cultists thirst for fresh blood.", 80, "Crimson Steps V", 0 },
                    { 332, 7, "Garnet cultists thirst for fresh blood.", 80, "Bleeding Stones V", 0 },
                    { 333, 7, "Garnet cultists thirst for fresh blood.", 80, "Hall of Offerings V", 0 },
                    { 334, 7, "Garnet cultists thirst for fresh blood.", 80, "Red Pools V", 0 },
                    { 335, 7, "Garnet cultists thirst for fresh blood.", 80, "Thorned Pillars V", 0 },
                    { 336, 7, "Garnet cultists thirst for fresh blood.", 80, "Cutter's Alley V", 0 },
                    { 337, 7, "Garnet cultists thirst for fresh blood.", 80, "Vein Tunnels V", 0 },
                    { 338, 7, "Garnet cultists thirst for fresh blood.", 81, "Garnet Court V", 0 },
                    { 339, 7, "Garnet cultists thirst for fresh blood.", 81, "Sacrificial Ring V", 0 },
                    { 340, 7, "Vargul the Bloodbound stands over the great altar.", 81, "The Great Altar V", 1 },
                    { 341, 1, "Fire tribes guard the forges deeper under the mountain.", 81, "Ashen Road V", 0 },
                    { 342, 1, "Fire tribes guard the forges deeper under the mountain.", 81, "Magma Falls V", 0 },
                    { 343, 1, "Fire tribes guard the forges deeper under the mountain.", 81, "Burning Bridge V", 0 },
                    { 344, 1, "Fire tribes guard the forges deeper under the mountain.", 81, "Slag Pits V", 0 },
                    { 345, 1, "Fire tribes guard the forges deeper under the mountain.", 81, "Obsidian Hall V", 0 },
                    { 346, 1, "Fire tribes guard the forges deeper under the mountain.", 82, "Ember Wastes V", 0 },
                    { 347, 1, "Fire tribes guard the forges deeper under the mountain.", 82, "Firebrand Camp V", 0 },
                    { 348, 1, "Fire tribes guard the forges deeper under the mountain.", 82, "Molten Vault V", 0 },
                    { 349, 1, "Fire tribes guard the forges deeper under the mountain.", 82, "Smelter's Hold V", 0 },
                    { 350, 1, "Ignar the Ashborn rises from the caldera.", 82, "Caldera Throne V", 1 },
                    { 351, 2, "Frozen hunters stalk the white wastes.", 82, "Snowblind Path V", 0 },
                    { 352, 2, "Frozen hunters stalk the white wastes.", 82, "Frostfang Gorge V", 0 },
                    { 353, 2, "Frozen hunters stalk the white wastes.", 82, "Frozen Lake V", 0 },
                    { 354, 2, "Frozen hunters stalk the white wastes.", 82, "Icicle Cave V", 0 },
                    { 355, 2, "Frozen hunters stalk the white wastes.", 83, "Hailstone Ridge V", 0 },
                    { 356, 2, "Frozen hunters stalk the white wastes.", 83, "Glacier Heart V", 0 },
                    { 357, 2, "Frozen hunters stalk the white wastes.", 83, "Whiteout Plains V", 0 },
                    { 358, 2, "Frozen hunters stalk the white wastes.", 83, "Rimehold Keep V", 0 },
                    { 359, 2, "Frozen hunters stalk the white wastes.", 83, "Avalanche Pass V", 0 },
                    { 360, 2, "Kryssa the Winter Drake guards the peak.", 83, "Peak of Winter V", 1 },
                    { 361, 3, "Storm creatures crackle with lightning on the slopes.", 83, "Thundering Steps VI", 0 },
                    { 362, 3, "Storm creatures crackle with lightning on the slopes.", 83, "Cloudbreak Ridge VI", 0 },
                    { 363, 3, "Storm creatures crackle with lightning on the slopes.", 83, "Spark Hollow VI", 0 },
                    { 364, 3, "Storm creatures crackle with lightning on the slopes.", 83, "Rumbling Cliffs VI", 0 },
                    { 365, 3, "Storm creatures crackle with lightning on the slopes.", 84, "Static Fields VI", 0 },
                    { 366, 3, "Storm creatures crackle with lightning on the slopes.", 84, "Lightning Spire VI", 0 },
                    { 367, 3, "Storm creatures crackle with lightning on the slopes.", 84, "Howling Saddle VI", 0 },
                    { 368, 3, "Storm creatures crackle with lightning on the slopes.", 84, "Stormwatch VI", 0 },
                    { 369, 3, "Storm creatures crackle with lightning on the slopes.", 84, "Skyforge Ruins VI", 0 },
                    { 370, 3, "Voltaris rides the storm above the summit.", 84, "Eye of the Storm VI", 1 },
                    { 371, 4, "Poison drips from every branch of the marsh.", 84, "Rotting Shallows VI", 0 },
                    { 372, 4, "Poison drips from every branch of the marsh.", 84, "Fen of Thorns VI", 0 },
                    { 373, 4, "Poison drips from every branch of the marsh.", 84, "Mossy Causeway VI", 0 },
                    { 374, 4, "Poison drips from every branch of the marsh.", 85, "Drowned Grove VI", 0 },
                    { 375, 4, "Poison drips from every branch of the marsh.", 85, "Toad Hollow VI", 0 },
                    { 376, 4, "Poison drips from every branch of the marsh.", 85, "Blackwater Bend VI", 0 },
                    { 377, 4, "Poison drips from every branch of the marsh.", 85, "Sporefield VI", 0 },
                    { 378, 4, "Poison drips from every branch of the marsh.", 85, "Hanging Roots VI", 0 },
                    { 379, 4, "Poison drips from every branch of the marsh.", 85, "Witchlight Bog VI", 0 },
                    { 380, 4, "Mirefang waits where the bog runs deepest.", 85, "Heart of the Mire VI", 1 },
                    { 381, 5, "The dead of the crypt rise against the intruders.", 85, "Silent Ossuary VI", 0 },
                    { 382, 5, "The dead of the crypt rise against the intruders.", 85, "Hall of Bones VI", 0 },
                    { 383, 5, "The dead of the crypt rise against the intruders.", 86, "Whispering Vault VI", 0 },
                    { 384, 5, "The dead of the crypt rise against the intruders.", 86, "Gravewind Stair VI", 0 },
                    { 385, 5, "The dead of the crypt rise against the intruders.", 86, "Chapel of Ash VI", 0 },
                    { 386, 5, "The dead of the crypt rise against the intruders.", 86, "Forgotten Catacombs VI", 0 },
                    { 387, 5, "The dead of the crypt rise against the intruders.", 86, "Mourners' Gallery VI", 0 },
                    { 388, 5, "The dead of the crypt rise against the intruders.", 86, "Sunless Depths VI", 0 },
                    { 389, 5, "The dead of the crypt rise against the intruders.", 86, "Sealed Reliquary VI", 0 },
                    { 390, 5, "Morvath the Hollow rules the deepest vault.", 86, "Throne of Night VI", 1 },
                    { 391, 6, "Crystal guardians shine with blinding light.", 86, "Prism Gate VI", 0 },
                    { 392, 6, "Crystal guardians shine with blinding light.", 86, "Shimmering Hall VI", 0 },
                    { 393, 6, "Crystal guardians shine with blinding light.", 87, "Glass Gardens VI", 0 },
                    { 394, 6, "Crystal guardians shine with blinding light.", 87, "Hall of Mirrors VI", 0 },
                    { 395, 6, "Crystal guardians shine with blinding light.", 87, "Choir of Light VI", 0 },
                    { 396, 6, "Crystal guardians shine with blinding light.", 87, "Radiant Nave VI", 0 },
                    { 397, 6, "Crystal guardians shine with blinding light.", 87, "Sunstone Bridge VI", 0 },
                    { 398, 6, "Crystal guardians shine with blinding light.", 87, "Gleaming Spire VI", 0 },
                    { 399, 6, "Crystal guardians shine with blinding light.", 87, "Starlit Archive VI", 0 },
                    { 400, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 87, "Crown of Dawn VI", 1 },
                    { 401, 7, "Garnet cultists thirst for fresh blood.", 87, "Crimson Steps VI", 0 },
                    { 402, 7, "Garnet cultists thirst for fresh blood.", 88, "Bleeding Stones VI", 0 },
                    { 403, 7, "Garnet cultists thirst for fresh blood.", 88, "Hall of Offerings VI", 0 },
                    { 404, 7, "Garnet cultists thirst for fresh blood.", 88, "Red Pools VI", 0 },
                    { 405, 7, "Garnet cultists thirst for fresh blood.", 88, "Thorned Pillars VI", 0 },
                    { 406, 7, "Garnet cultists thirst for fresh blood.", 88, "Cutter's Alley VI", 0 },
                    { 407, 7, "Garnet cultists thirst for fresh blood.", 88, "Vein Tunnels VI", 0 },
                    { 408, 7, "Garnet cultists thirst for fresh blood.", 88, "Garnet Court VI", 0 },
                    { 409, 7, "Garnet cultists thirst for fresh blood.", 88, "Sacrificial Ring VI", 0 },
                    { 410, 7, "Vargul the Bloodbound stands over the great altar.", 88, "The Great Altar VI", 1 },
                    { 411, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Ashen Road VI", 0 },
                    { 412, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Magma Falls VI", 0 },
                    { 413, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Burning Bridge VI", 0 },
                    { 414, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Slag Pits VI", 0 },
                    { 415, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Obsidian Hall VI", 0 },
                    { 416, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Ember Wastes VI", 0 },
                    { 417, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Firebrand Camp VI", 0 },
                    { 418, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Molten Vault VI", 0 },
                    { 419, 1, "Fire tribes guard the forges deeper under the mountain.", 89, "Smelter's Hold VI", 0 },
                    { 420, 1, "Ignar the Ashborn rises from the caldera.", 89, "Caldera Throne VI", 1 },
                    { 421, 2, "Frozen hunters stalk the white wastes.", 90, "Snowblind Path VI", 0 },
                    { 422, 2, "Frozen hunters stalk the white wastes.", 90, "Frostfang Gorge VI", 0 },
                    { 423, 2, "Frozen hunters stalk the white wastes.", 90, "Frozen Lake VI", 0 },
                    { 424, 2, "Frozen hunters stalk the white wastes.", 90, "Icicle Cave VI", 0 },
                    { 425, 2, "Frozen hunters stalk the white wastes.", 90, "Hailstone Ridge VI", 0 },
                    { 426, 2, "Frozen hunters stalk the white wastes.", 90, "Glacier Heart VI", 0 },
                    { 427, 2, "Frozen hunters stalk the white wastes.", 90, "Whiteout Plains VI", 0 },
                    { 428, 2, "Frozen hunters stalk the white wastes.", 90, "Rimehold Keep VI", 0 },
                    { 429, 2, "Frozen hunters stalk the white wastes.", 90, "Avalanche Pass VI", 0 },
                    { 430, 2, "Kryssa the Winter Drake guards the peak.", 91, "Peak of Winter VI", 1 },
                    { 431, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Thundering Steps VII", 0 },
                    { 432, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Cloudbreak Ridge VII", 0 },
                    { 433, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Spark Hollow VII", 0 },
                    { 434, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Rumbling Cliffs VII", 0 },
                    { 435, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Static Fields VII", 0 },
                    { 436, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Lightning Spire VII", 0 },
                    { 437, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Howling Saddle VII", 0 },
                    { 438, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Stormwatch VII", 0 },
                    { 439, 3, "Storm creatures crackle with lightning on the slopes.", 91, "Skyforge Ruins VII", 0 },
                    { 440, 3, "Voltaris rides the storm above the summit.", 92, "Eye of the Storm VII", 1 },
                    { 441, 4, "Poison drips from every branch of the marsh.", 92, "Rotting Shallows VII", 0 },
                    { 442, 4, "Poison drips from every branch of the marsh.", 92, "Fen of Thorns VII", 0 },
                    { 443, 4, "Poison drips from every branch of the marsh.", 92, "Mossy Causeway VII", 0 },
                    { 444, 4, "Poison drips from every branch of the marsh.", 92, "Drowned Grove VII", 0 },
                    { 445, 4, "Poison drips from every branch of the marsh.", 92, "Toad Hollow VII", 0 },
                    { 446, 4, "Poison drips from every branch of the marsh.", 92, "Blackwater Bend VII", 0 },
                    { 447, 4, "Poison drips from every branch of the marsh.", 92, "Sporefield VII", 0 },
                    { 448, 4, "Poison drips from every branch of the marsh.", 92, "Hanging Roots VII", 0 },
                    { 449, 4, "Poison drips from every branch of the marsh.", 93, "Witchlight Bog VII", 0 },
                    { 450, 4, "Mirefang waits where the bog runs deepest.", 93, "Heart of the Mire VII", 1 },
                    { 451, 5, "The dead of the crypt rise against the intruders.", 93, "Silent Ossuary VII", 0 },
                    { 452, 5, "The dead of the crypt rise against the intruders.", 93, "Hall of Bones VII", 0 },
                    { 453, 5, "The dead of the crypt rise against the intruders.", 93, "Whispering Vault VII", 0 },
                    { 454, 5, "The dead of the crypt rise against the intruders.", 93, "Gravewind Stair VII", 0 },
                    { 455, 5, "The dead of the crypt rise against the intruders.", 93, "Chapel of Ash VII", 0 },
                    { 456, 5, "The dead of the crypt rise against the intruders.", 93, "Forgotten Catacombs VII", 0 },
                    { 457, 5, "The dead of the crypt rise against the intruders.", 93, "Mourners' Gallery VII", 0 },
                    { 458, 5, "The dead of the crypt rise against the intruders.", 94, "Sunless Depths VII", 0 },
                    { 459, 5, "The dead of the crypt rise against the intruders.", 94, "Sealed Reliquary VII", 0 },
                    { 460, 5, "Morvath the Hollow rules the deepest vault.", 94, "Throne of Night VII", 1 },
                    { 461, 6, "Crystal guardians shine with blinding light.", 94, "Prism Gate VII", 0 },
                    { 462, 6, "Crystal guardians shine with blinding light.", 94, "Shimmering Hall VII", 0 },
                    { 463, 6, "Crystal guardians shine with blinding light.", 94, "Glass Gardens VII", 0 },
                    { 464, 6, "Crystal guardians shine with blinding light.", 94, "Hall of Mirrors VII", 0 },
                    { 465, 6, "Crystal guardians shine with blinding light.", 94, "Choir of Light VII", 0 },
                    { 466, 6, "Crystal guardians shine with blinding light.", 94, "Radiant Nave VII", 0 },
                    { 467, 6, "Crystal guardians shine with blinding light.", 94, "Sunstone Bridge VII", 0 },
                    { 468, 6, "Crystal guardians shine with blinding light.", 95, "Gleaming Spire VII", 0 },
                    { 469, 6, "Crystal guardians shine with blinding light.", 95, "Starlit Archive VII", 0 },
                    { 470, 6, "Aurelion the Shining blazes at the heart of the sanctum.", 95, "Crown of Dawn VII", 1 },
                    { 471, 7, "Garnet cultists thirst for fresh blood.", 95, "Crimson Steps VII", 0 },
                    { 472, 7, "Garnet cultists thirst for fresh blood.", 95, "Bleeding Stones VII", 0 },
                    { 473, 7, "Garnet cultists thirst for fresh blood.", 95, "Hall of Offerings VII", 0 },
                    { 474, 7, "Garnet cultists thirst for fresh blood.", 95, "Red Pools VII", 0 },
                    { 475, 7, "Garnet cultists thirst for fresh blood.", 95, "Thorned Pillars VII", 0 },
                    { 476, 7, "Garnet cultists thirst for fresh blood.", 95, "Cutter's Alley VII", 0 },
                    { 477, 7, "Garnet cultists thirst for fresh blood.", 96, "Vein Tunnels VII", 0 },
                    { 478, 7, "Garnet cultists thirst for fresh blood.", 96, "Garnet Court VII", 0 },
                    { 479, 7, "Garnet cultists thirst for fresh blood.", 96, "Sacrificial Ring VII", 0 },
                    { 480, 7, "Vargul the Bloodbound stands over the great altar.", 96, "The Great Altar VII", 1 },
                    { 481, 1, "Fire tribes guard the forges deeper under the mountain.", 96, "Ashen Road VII", 0 },
                    { 482, 1, "Fire tribes guard the forges deeper under the mountain.", 96, "Magma Falls VII", 0 },
                    { 483, 1, "Fire tribes guard the forges deeper under the mountain.", 96, "Burning Bridge VII", 0 },
                    { 484, 1, "Fire tribes guard the forges deeper under the mountain.", 96, "Slag Pits VII", 0 },
                    { 485, 1, "Fire tribes guard the forges deeper under the mountain.", 96, "Obsidian Hall VII", 0 },
                    { 486, 1, "Fire tribes guard the forges deeper under the mountain.", 97, "Ember Wastes VII", 0 },
                    { 487, 1, "Fire tribes guard the forges deeper under the mountain.", 97, "Firebrand Camp VII", 0 },
                    { 488, 1, "Fire tribes guard the forges deeper under the mountain.", 97, "Molten Vault VII", 0 },
                    { 489, 1, "Fire tribes guard the forges deeper under the mountain.", 97, "Smelter's Hold VII", 0 },
                    { 490, 1, "Ignar the Ashborn rises from the caldera.", 97, "Caldera Throne VII", 1 },
                    { 491, 2, "Frozen hunters stalk the white wastes.", 97, "Snowblind Path VII", 0 },
                    { 492, 2, "Frozen hunters stalk the white wastes.", 97, "Frostfang Gorge VII", 0 },
                    { 493, 2, "Frozen hunters stalk the white wastes.", 97, "Frozen Lake VII", 0 },
                    { 494, 2, "Frozen hunters stalk the white wastes.", 97, "Icicle Cave VII", 0 },
                    { 495, 2, "Frozen hunters stalk the white wastes.", 97, "Hailstone Ridge VII", 0 },
                    { 496, 2, "Frozen hunters stalk the white wastes.", 98, "Glacier Heart VII", 0 },
                    { 497, 2, "Frozen hunters stalk the white wastes.", 98, "Whiteout Plains VII", 0 },
                    { 498, 2, "Frozen hunters stalk the white wastes.", 98, "Rimehold Keep VII", 0 },
                    { 499, 2, "Frozen hunters stalk the white wastes.", 98, "Avalanche Pass VII", 0 },
                    { 500, 2, "Kryssa the Winter Drake guards the peak.", 98, "Peak of Winter VII", 1 }
                });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Name", "UnitType" },
                values: new object[] { "Magma Golem", "Golem" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Name", "UnitType" },
                values: new object[] { "Ember Drake", "Draconid" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "UnitType",
                value: "Draconid");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                column: "UnitType",
                value: "Golem");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                column: "UnitType",
                value: "Golem");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                column: "UnitType",
                value: "Draconid");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                column: "UnitType",
                value: "Golem");

            migrationBuilder.InsertData(
                table: "Heroes",
                columns: new[] { "Id", "Attack", "AuraDuration", "AuraEffect", "AuraInterval", "AuraLevel", "AuraPower", "AuraTarget", "AuraTrigger", "Element", "EnergyPerAttack", "Immunities", "Level", "MaxEnergy", "MaxHp", "Name", "PassiveLevel", "Passives", "PlaceInSquad", "PlayerId", "Profession", "Speed", "SpellIds", "StartEnergy", "UnitType", "UpgradeLevel" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-4000-8000-000000000201"), 70, 1, null, 1, 1, 0, 0, 1, "Lightning", 20, "Shock", 4, 100, 1000, "Storm Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000202"), 85, 1, null, 1, 1, 0, 0, 1, "Lightning", 20, "", 4, 100, 520, "Spark Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000203"), 80, 1, null, 1, 1, 0, 0, 1, "Lightning", 20, "", 4, 100, 500, "Thunder Skeleton", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "4", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000204"), 95, 1, null, 1, 1, 0, 0, 1, "Lightning", 20, "Shock", 4, 100, 760, "Storm Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "25", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000205"), 120, 1, "Shock", 1, 1, 8, 1, 1, "Lightning", 20, "Shock,Stun", 4, 100, 1400, "Voltaris the Storm Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "28", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000211"), 70, 1, null, 1, 1, 0, 0, 1, "Nature", 20, "Poison", 4, 100, 1000, "Moss Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000212"), 85, 1, null, 1, 1, 0, 0, 1, "Nature", 20, "", 4, 100, 520, "Bog Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000213"), 80, 1, null, 1, 1, 0, 0, 1, "Nature", 20, "", 4, 100, 500, "Thorn Skeleton", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "31", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000214"), 95, 1, null, 1, 1, 0, 0, 1, "Nature", 20, "Poison", 4, 100, 760, "Swamp Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "34", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000215"), 120, 1, "Poison", 1, 1, 8, 1, 1, "Nature", 20, "Poison,Stun", 4, 100, 1400, "Mirefang the Rotting", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "32", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000221"), 70, 1, null, 1, 1, 0, 0, 1, "Dark", 20, "Curse", 4, 100, 1000, "Grave Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000222"), 85, 1, null, 1, 1, 0, 0, 1, "Dark", 20, "", 4, 100, 520, "Shade Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000223"), 80, 1, null, 1, 1, 0, 0, 1, "Dark", 20, "", 4, 100, 500, "Hex Skeleton", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "43", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000224"), 95, 1, null, 1, 1, 0, 0, 1, "Dark", 20, "Curse", 4, 100, 760, "Night Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "37", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000225"), 120, 1, "Curse", 1, 1, 8, 1, 1, "Dark", 20, "Curse,Stun", 4, 100, 1400, "Morvath the Hollow", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "55", 0, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000231"), 70, 1, null, 1, 1, 0, 0, 1, "Radiance", 20, "Stun", 4, 100, 1000, "Crystal Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000232"), 85, 1, null, 1, 1, 0, 0, 1, "Radiance", 20, "", 4, 100, 520, "Prism Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000233"), 80, 1, null, 1, 1, 0, 0, 1, "Radiance", 20, "", 4, 100, 500, "Dawn Skeleton", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "52", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000234"), 95, 1, null, 1, 1, 0, 0, 1, "Radiance", 20, "Stun", 4, 100, 760, "Radiant Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "49", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000235"), 120, 1, "Regen", 1, 1, 10, 0, 1, "Radiance", 20, "Stun,Stun", 4, 100, 1400, "Aurelion the Shining", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "47", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000241"), 70, 1, null, 1, 1, 0, 0, 1, "Blood", 20, "Bleed", 4, 100, 1000, "Flesh Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000242"), 85, 1, null, 1, 1, 0, 0, 1, "Blood", 20, "", 4, 100, 520, "Leech Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000243"), 80, 1, null, 1, 1, 0, 0, 1, "Blood", 20, "", 4, 100, 500, "Bone Reaver", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "58", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000244"), 95, 1, null, 1, 1, 0, 0, 1, "Blood", 20, "Bleed", 4, 100, 760, "Blood Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "61", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000245"), 120, 1, "Bleed", 1, 1, 8, 1, 1, "Blood", 20, "Bleed,Stun", 4, 100, 1400, "Vargul the Bloodbound", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "59", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000255"), 120, 1, "Burn", 1, 1, 8, 1, 1, "Fire", 20, "Burn,Stun", 4, 100, 1400, "Ignar the Ashborn", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "11", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000261"), 70, 1, null, 1, 1, 0, 0, 1, "Frost", 20, "Frost", 4, 100, 1000, "Glacier Golem", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 3, "", 0, "Golem", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000262"), 85, 1, null, 1, 1, 0, 0, 1, "Frost", 20, "", 4, 100, 520, "Snow Goblin", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 8, "", 0, "Goblin", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000263"), 80, 1, null, 1, 1, 0, 0, 1, "Frost", 20, "", 4, 100, 500, "Frost Skeleton", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 6, "16", 20, "Skeleton", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000264"), 95, 1, null, 1, 1, 0, 0, 1, "Frost", 20, "Frost", 4, 100, 760, "Frost Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "19", 0, "Draconid", 1 },
                    { new Guid("33333333-0000-4000-8000-000000000265"), 120, 1, "Frost", 1, 1, 5, 1, 1, "Frost", 20, "Frost,Stun", 4, 100, 1400, "Kryssa the Winter Drake", 1, "", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), null, 5, "23", 0, "Draconid", 1 }
                });

            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[,]
                {
                    { 1101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 11 },
                    { 1102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 11 },
                    { 1103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 11 },
                    { 1201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 12 },
                    { 1202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 12 },
                    { 1203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 12 },
                    { 1301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 13 },
                    { 1302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 13 },
                    { 1303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 13 },
                    { 1401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 14 },
                    { 1402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 14 },
                    { 1403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 14 },
                    { 1404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 14 },
                    { 1501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 15 },
                    { 1502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 15 },
                    { 1503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 15 },
                    { 1504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 15 },
                    { 1601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 16 },
                    { 1602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 16 },
                    { 1603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 16 },
                    { 1604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 16 },
                    { 1701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 17 },
                    { 1702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 17 },
                    { 1703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 17 },
                    { 1704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 17 },
                    { 1705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 17 },
                    { 1801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 18 },
                    { 1802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 18 },
                    { 1803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 18 },
                    { 1804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 18 },
                    { 1805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 18 },
                    { 1901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 19 },
                    { 1902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 19 },
                    { 1903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 19 },
                    { 1904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 19 },
                    { 1905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 19 },
                    { 2001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 20 },
                    { 2002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 20 },
                    { 2003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 20 },
                    { 2004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 20 },
                    { 2101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 21 },
                    { 2102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 21 },
                    { 2103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 21 },
                    { 2201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 22 },
                    { 2202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 22 },
                    { 2203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 22 },
                    { 2301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 23 },
                    { 2302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 23 },
                    { 2303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 23 },
                    { 2401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 24 },
                    { 2402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 24 },
                    { 2403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 24 },
                    { 2404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 24 },
                    { 2501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 25 },
                    { 2502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 25 },
                    { 2503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 25 },
                    { 2504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 25 },
                    { 2601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 26 },
                    { 2602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 26 },
                    { 2603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 26 },
                    { 2604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 26 },
                    { 2701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 27 },
                    { 2702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 27 },
                    { 2703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 27 },
                    { 2704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 27 },
                    { 2705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 27 },
                    { 2801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 28 },
                    { 2802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 28 },
                    { 2803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 28 },
                    { 2804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 28 },
                    { 2805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 28 },
                    { 2901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 29 },
                    { 2902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 29 },
                    { 2903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 29 },
                    { 2904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 29 },
                    { 2905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 29 },
                    { 3001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 30 },
                    { 3002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 30 },
                    { 3003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 30 },
                    { 3004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 30 },
                    { 3101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 31 },
                    { 3102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 31 },
                    { 3103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 31 },
                    { 3201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 32 },
                    { 3202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 32 },
                    { 3203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 32 },
                    { 3301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 33 },
                    { 3302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 33 },
                    { 3303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 33 },
                    { 3401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 34 },
                    { 3402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 34 },
                    { 3403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 34 },
                    { 3404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 34 },
                    { 3501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 35 },
                    { 3502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 35 },
                    { 3503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 35 },
                    { 3504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 35 },
                    { 3601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 36 },
                    { 3602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 36 },
                    { 3603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 36 },
                    { 3604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 36 },
                    { 3701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 37 },
                    { 3702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 37 },
                    { 3703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 37 },
                    { 3704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 37 },
                    { 3705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 37 },
                    { 3801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 38 },
                    { 3802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 38 },
                    { 3803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 38 },
                    { 3804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 38 },
                    { 3805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 38 },
                    { 3901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 39 },
                    { 3902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 39 },
                    { 3903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 39 },
                    { 3904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 39 },
                    { 3905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 39 },
                    { 4001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 40 },
                    { 4002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 40 },
                    { 4003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 40 },
                    { 4004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 40 },
                    { 4101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 41 },
                    { 4102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 41 },
                    { 4103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 41 },
                    { 4201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 42 },
                    { 4202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 42 },
                    { 4203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 42 },
                    { 4301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 43 },
                    { 4302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 43 },
                    { 4303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 43 },
                    { 4401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 44 },
                    { 4402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 44 },
                    { 4403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 44 },
                    { 4404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 44 },
                    { 4501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 45 },
                    { 4502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 45 },
                    { 4503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 45 },
                    { 4504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 45 },
                    { 4601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 46 },
                    { 4602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 46 },
                    { 4603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 46 },
                    { 4604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 46 },
                    { 4701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 47 },
                    { 4702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 47 },
                    { 4703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 47 },
                    { 4704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 47 },
                    { 4705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 47 },
                    { 4801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 48 },
                    { 4802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 48 },
                    { 4803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 48 },
                    { 4804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 48 },
                    { 4805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 48 },
                    { 4901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 49 },
                    { 4902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 49 },
                    { 4903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 49 },
                    { 4904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 49 },
                    { 4905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 49 },
                    { 5001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 50 },
                    { 5002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 50 },
                    { 5003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 50 },
                    { 5004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 50 },
                    { 5101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 51 },
                    { 5102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 51 },
                    { 5103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 51 },
                    { 5201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 52 },
                    { 5202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 52 },
                    { 5203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 52 },
                    { 5301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 53 },
                    { 5302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 53 },
                    { 5303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 53 },
                    { 5401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 54 },
                    { 5402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 54 },
                    { 5403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 54 },
                    { 5404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 54 },
                    { 5501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 55 },
                    { 5502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 55 },
                    { 5503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 55 },
                    { 5504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 55 },
                    { 5601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 56 },
                    { 5602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 56 },
                    { 5603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 56 },
                    { 5604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 56 },
                    { 5701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 57 },
                    { 5702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 57 },
                    { 5703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 57 },
                    { 5704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 57 },
                    { 5705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 57 },
                    { 5801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 58 },
                    { 5802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 58 },
                    { 5803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 58 },
                    { 5804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 58 },
                    { 5805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 58 },
                    { 5901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 59 },
                    { 5902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 59 },
                    { 5903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 59 },
                    { 5904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 59 },
                    { 5905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 59 },
                    { 6001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 60 },
                    { 6002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 60 },
                    { 6003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 60 },
                    { 6004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 60 },
                    { 6101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 61 },
                    { 6102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 61 },
                    { 6103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 61 },
                    { 6201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 62 },
                    { 6202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 62 },
                    { 6203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 62 },
                    { 6301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 63 },
                    { 6302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 63 },
                    { 6303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 63 },
                    { 6401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 64 },
                    { 6402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 64 },
                    { 6403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 64 },
                    { 6404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 64 },
                    { 6501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 65 },
                    { 6502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 65 },
                    { 6503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 65 },
                    { 6504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 65 },
                    { 6601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 66 },
                    { 6602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 66 },
                    { 6603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 66 },
                    { 6604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 66 },
                    { 6701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 67 },
                    { 6702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 67 },
                    { 6703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 67 },
                    { 6704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 67 },
                    { 6705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 67 },
                    { 6801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 68 },
                    { 6802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 68 },
                    { 6803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 68 },
                    { 6804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 68 },
                    { 6805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 68 },
                    { 6901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 69 },
                    { 6902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 69 },
                    { 6903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 69 },
                    { 6904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 69 },
                    { 6905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 69 },
                    { 7001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 70 },
                    { 7002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 70 },
                    { 7003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 70 },
                    { 7004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 70 },
                    { 7101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 71 },
                    { 7102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 71 },
                    { 7103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 71 },
                    { 7201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 72 },
                    { 7202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 72 },
                    { 7203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 72 },
                    { 7301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 73 },
                    { 7302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 73 },
                    { 7303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 73 },
                    { 7401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 74 },
                    { 7402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 74 },
                    { 7403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 74 },
                    { 7404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 74 },
                    { 7501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 75 },
                    { 7502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 75 },
                    { 7503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 75 },
                    { 7504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 75 },
                    { 7601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 76 },
                    { 7602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 76 },
                    { 7603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 76 },
                    { 7604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 76 },
                    { 7701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 77 },
                    { 7702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 77 },
                    { 7703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 77 },
                    { 7704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 77 },
                    { 7705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 77 },
                    { 7801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 78 },
                    { 7802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 78 },
                    { 7803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 78 },
                    { 7804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 78 },
                    { 7805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 78 },
                    { 7901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 79 },
                    { 7902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 79 },
                    { 7903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 79 },
                    { 7904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 79 },
                    { 7905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 79 },
                    { 8001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 80 },
                    { 8002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 80 },
                    { 8003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 80 },
                    { 8004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 80 },
                    { 8101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 81 },
                    { 8102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 81 },
                    { 8103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 81 },
                    { 8201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 82 },
                    { 8202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 82 },
                    { 8203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 82 },
                    { 8301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 83 },
                    { 8302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 83 },
                    { 8303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 83 },
                    { 8401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 84 },
                    { 8402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 84 },
                    { 8403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 84 },
                    { 8404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 84 },
                    { 8501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 85 },
                    { 8502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 85 },
                    { 8503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 85 },
                    { 8504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 85 },
                    { 8601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 86 },
                    { 8602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 86 },
                    { 8603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 86 },
                    { 8604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 86 },
                    { 8701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 87 },
                    { 8702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 87 },
                    { 8703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 87 },
                    { 8704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 87 },
                    { 8705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 87 },
                    { 8801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 88 },
                    { 8802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 88 },
                    { 8803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 88 },
                    { 8804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 88 },
                    { 8805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 88 },
                    { 8901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 89 },
                    { 8902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 89 },
                    { 8903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 89 },
                    { 8904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 89 },
                    { 8905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 89 },
                    { 9001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 90 },
                    { 9002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 90 },
                    { 9003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 90 },
                    { 9004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 90 },
                    { 9101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 91 },
                    { 9102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 91 },
                    { 9103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 91 },
                    { 9201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 92 },
                    { 9202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 92 },
                    { 9203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 92 },
                    { 9301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 93 },
                    { 9302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 93 },
                    { 9303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 93 },
                    { 9401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 94 },
                    { 9402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 94 },
                    { 9403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 94 },
                    { 9404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 94 },
                    { 9501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 95 },
                    { 9502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 95 },
                    { 9503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 95 },
                    { 9504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 95 },
                    { 9601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 96 },
                    { 9602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 96 },
                    { 9603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 96 },
                    { 9604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 96 },
                    { 9701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 97 },
                    { 9702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 97 },
                    { 9703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 97 },
                    { 9704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 97 },
                    { 9705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 97 },
                    { 9801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 98 },
                    { 9802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 98 },
                    { 9803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 98 },
                    { 9804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 98 },
                    { 9805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 98 },
                    { 9901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 99 },
                    { 9902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 99 },
                    { 9903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 99 },
                    { 9904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 99 },
                    { 9905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 99 },
                    { 10001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 100 },
                    { 10002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 100 },
                    { 10003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 100 },
                    { 10004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 100 },
                    { 10101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 101 },
                    { 10102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 101 },
                    { 10103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 101 },
                    { 10201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 102 },
                    { 10202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 102 },
                    { 10203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 102 },
                    { 10301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 103 },
                    { 10302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 103 },
                    { 10303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 103 },
                    { 10401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 104 },
                    { 10402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 104 },
                    { 10403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 104 },
                    { 10404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 104 },
                    { 10501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 105 },
                    { 10502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 105 },
                    { 10503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 105 },
                    { 10504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 105 },
                    { 10601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 106 },
                    { 10602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 106 },
                    { 10603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 106 },
                    { 10604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 106 },
                    { 10701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 107 },
                    { 10702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 107 },
                    { 10703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 107 },
                    { 10704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 107 },
                    { 10705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 107 },
                    { 10801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 108 },
                    { 10802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 108 },
                    { 10803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 108 },
                    { 10804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 108 },
                    { 10805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 108 },
                    { 10901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 109 },
                    { 10902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 109 },
                    { 10903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 109 },
                    { 10904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 109 },
                    { 10905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 109 },
                    { 11001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 110 },
                    { 11002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 110 },
                    { 11003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 110 },
                    { 11004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 110 },
                    { 11101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 111 },
                    { 11102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 111 },
                    { 11103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 111 },
                    { 11201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 112 },
                    { 11202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 112 },
                    { 11203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 112 },
                    { 11301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 113 },
                    { 11302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 113 },
                    { 11303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 113 },
                    { 11401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 114 },
                    { 11402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 114 },
                    { 11403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 114 },
                    { 11404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 114 },
                    { 11501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 115 },
                    { 11502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 115 },
                    { 11503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 115 },
                    { 11504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 115 },
                    { 11601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 116 },
                    { 11602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 116 },
                    { 11603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 116 },
                    { 11604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 116 },
                    { 11701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 117 },
                    { 11702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 117 },
                    { 11703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 117 },
                    { 11704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 117 },
                    { 11705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 117 },
                    { 11801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 118 },
                    { 11802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 118 },
                    { 11803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 118 },
                    { 11804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 118 },
                    { 11805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 118 },
                    { 11901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 119 },
                    { 11902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 119 },
                    { 11903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 119 },
                    { 11904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 119 },
                    { 11905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 119 },
                    { 12001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 120 },
                    { 12002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 120 },
                    { 12003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 120 },
                    { 12004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 120 },
                    { 12101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 121 },
                    { 12102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 121 },
                    { 12103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 121 },
                    { 12201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 122 },
                    { 12202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 122 },
                    { 12203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 122 },
                    { 12301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 123 },
                    { 12302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 123 },
                    { 12303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 123 },
                    { 12401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 124 },
                    { 12402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 124 },
                    { 12403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 124 },
                    { 12404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 124 },
                    { 12501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 125 },
                    { 12502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 125 },
                    { 12503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 125 },
                    { 12504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 125 },
                    { 12601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 126 },
                    { 12602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 126 },
                    { 12603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 126 },
                    { 12604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 126 },
                    { 12701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 127 },
                    { 12702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 127 },
                    { 12703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 127 },
                    { 12704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 127 },
                    { 12705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 127 },
                    { 12801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 128 },
                    { 12802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 128 },
                    { 12803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 128 },
                    { 12804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 128 },
                    { 12805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 128 },
                    { 12901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 129 },
                    { 12902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 129 },
                    { 12903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 129 },
                    { 12904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 129 },
                    { 12905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 129 },
                    { 13001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 130 },
                    { 13002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 130 },
                    { 13003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 130 },
                    { 13004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 130 },
                    { 13101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 131 },
                    { 13102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 131 },
                    { 13103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 131 },
                    { 13201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 132 },
                    { 13202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 132 },
                    { 13203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 132 },
                    { 13301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 133 },
                    { 13302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 133 },
                    { 13303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 133 },
                    { 13401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 134 },
                    { 13402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 134 },
                    { 13403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 134 },
                    { 13404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 134 },
                    { 13501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 135 },
                    { 13502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 135 },
                    { 13503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 135 },
                    { 13504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 135 },
                    { 13601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 136 },
                    { 13602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 136 },
                    { 13603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 136 },
                    { 13604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 136 },
                    { 13701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 137 },
                    { 13702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 137 },
                    { 13703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 137 },
                    { 13704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 137 },
                    { 13705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 137 },
                    { 13801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 138 },
                    { 13802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 138 },
                    { 13803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 138 },
                    { 13804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 138 },
                    { 13805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 138 },
                    { 13901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 139 },
                    { 13902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 139 },
                    { 13903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 139 },
                    { 13904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 139 },
                    { 13905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 139 },
                    { 14001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 140 },
                    { 14002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 140 },
                    { 14003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 140 },
                    { 14004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 140 },
                    { 14101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 141 },
                    { 14102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 141 },
                    { 14103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 141 },
                    { 14201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 142 },
                    { 14202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 142 },
                    { 14203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 142 },
                    { 14301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 143 },
                    { 14302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 143 },
                    { 14303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 143 },
                    { 14401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 144 },
                    { 14402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 144 },
                    { 14403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 144 },
                    { 14404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 144 },
                    { 14501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 145 },
                    { 14502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 145 },
                    { 14503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 145 },
                    { 14504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 145 },
                    { 14601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 146 },
                    { 14602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 146 },
                    { 14603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 146 },
                    { 14604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 146 },
                    { 14701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 147 },
                    { 14702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 147 },
                    { 14703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 147 },
                    { 14704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 147 },
                    { 14705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 147 },
                    { 14801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 148 },
                    { 14802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 148 },
                    { 14803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 148 },
                    { 14804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 148 },
                    { 14805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 148 },
                    { 14901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 149 },
                    { 14902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 149 },
                    { 14903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 149 },
                    { 14904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 149 },
                    { 14905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 149 },
                    { 15001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 150 },
                    { 15002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 150 },
                    { 15003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 150 },
                    { 15004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 150 },
                    { 15101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 151 },
                    { 15102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 151 },
                    { 15103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 151 },
                    { 15201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 152 },
                    { 15202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 152 },
                    { 15203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 152 },
                    { 15301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 153 },
                    { 15302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 153 },
                    { 15303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 153 },
                    { 15401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 154 },
                    { 15402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 154 },
                    { 15403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 154 },
                    { 15404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 154 },
                    { 15501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 155 },
                    { 15502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 155 },
                    { 15503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 155 },
                    { 15504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 155 },
                    { 15601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 156 },
                    { 15602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 156 },
                    { 15603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 156 },
                    { 15604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 156 },
                    { 15701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 157 },
                    { 15702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 157 },
                    { 15703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 157 },
                    { 15704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 157 },
                    { 15705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 157 },
                    { 15801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 158 },
                    { 15802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 158 },
                    { 15803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 158 },
                    { 15804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 158 },
                    { 15805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 158 },
                    { 15901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 159 },
                    { 15902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 159 },
                    { 15903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 159 },
                    { 15904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 159 },
                    { 15905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 159 },
                    { 16001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 160 },
                    { 16002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 160 },
                    { 16003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 160 },
                    { 16004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 160 },
                    { 16101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 161 },
                    { 16102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 161 },
                    { 16103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 161 },
                    { 16201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 162 },
                    { 16202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 162 },
                    { 16203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 162 },
                    { 16301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 163 },
                    { 16302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 163 },
                    { 16303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 163 },
                    { 16401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 164 },
                    { 16402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 164 },
                    { 16403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 164 },
                    { 16404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 164 },
                    { 16501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 165 },
                    { 16502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 165 },
                    { 16503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 165 },
                    { 16504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 165 },
                    { 16601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 166 },
                    { 16602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 166 },
                    { 16603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 166 },
                    { 16604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 166 },
                    { 16701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 167 },
                    { 16702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 167 },
                    { 16703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 167 },
                    { 16704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 167 },
                    { 16705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 167 },
                    { 16801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 168 },
                    { 16802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 168 },
                    { 16803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 168 },
                    { 16804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 168 },
                    { 16805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 168 },
                    { 16901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 169 },
                    { 16902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 169 },
                    { 16903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 169 },
                    { 16904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 169 },
                    { 16905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 169 },
                    { 17001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 170 },
                    { 17002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 170 },
                    { 17003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 170 },
                    { 17004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 170 },
                    { 17101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 171 },
                    { 17102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 171 },
                    { 17103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 171 },
                    { 17201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 172 },
                    { 17202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 172 },
                    { 17203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 172 },
                    { 17301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 173 },
                    { 17302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 173 },
                    { 17303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 173 },
                    { 17401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 174 },
                    { 17402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 174 },
                    { 17403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 174 },
                    { 17404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 174 },
                    { 17501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 175 },
                    { 17502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 175 },
                    { 17503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 175 },
                    { 17504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 175 },
                    { 17601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 176 },
                    { 17602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 176 },
                    { 17603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 176 },
                    { 17604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 176 },
                    { 17701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 177 },
                    { 17702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 177 },
                    { 17703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 177 },
                    { 17704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 177 },
                    { 17705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 177 },
                    { 17801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 178 },
                    { 17802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 178 },
                    { 17803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 178 },
                    { 17804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 178 },
                    { 17805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 178 },
                    { 17901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 179 },
                    { 17902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 179 },
                    { 17903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 179 },
                    { 17904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 179 },
                    { 17905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 179 },
                    { 18001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 180 },
                    { 18002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 180 },
                    { 18003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 180 },
                    { 18004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 180 },
                    { 18101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 181 },
                    { 18102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 181 },
                    { 18103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 181 },
                    { 18201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 182 },
                    { 18202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 182 },
                    { 18203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 182 },
                    { 18301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 183 },
                    { 18302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 183 },
                    { 18303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 183 },
                    { 18401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 184 },
                    { 18402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 184 },
                    { 18403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 184 },
                    { 18404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 184 },
                    { 18501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 185 },
                    { 18502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 185 },
                    { 18503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 185 },
                    { 18504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 185 },
                    { 18601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 186 },
                    { 18602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 186 },
                    { 18603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 186 },
                    { 18604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 186 },
                    { 18701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 187 },
                    { 18702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 187 },
                    { 18703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 187 },
                    { 18704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 187 },
                    { 18705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 187 },
                    { 18801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 188 },
                    { 18802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 188 },
                    { 18803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 188 },
                    { 18804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 188 },
                    { 18805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 188 },
                    { 18901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 189 },
                    { 18902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 189 },
                    { 18903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 189 },
                    { 18904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 189 },
                    { 18905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 189 },
                    { 19001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 190 },
                    { 19002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 190 },
                    { 19003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 190 },
                    { 19004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 190 },
                    { 19101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 191 },
                    { 19102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 191 },
                    { 19103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 191 },
                    { 19201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 192 },
                    { 19202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 192 },
                    { 19203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 192 },
                    { 19301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 193 },
                    { 19302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 193 },
                    { 19303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 193 },
                    { 19401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 194 },
                    { 19402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 194 },
                    { 19403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 194 },
                    { 19404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 194 },
                    { 19501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 195 },
                    { 19502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 195 },
                    { 19503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 195 },
                    { 19504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 195 },
                    { 19601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 196 },
                    { 19602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 196 },
                    { 19603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 196 },
                    { 19604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 196 },
                    { 19701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 197 },
                    { 19702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 197 },
                    { 19703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 197 },
                    { 19704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 197 },
                    { 19705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 197 },
                    { 19801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 198 },
                    { 19802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 198 },
                    { 19803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 198 },
                    { 19804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 198 },
                    { 19805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 198 },
                    { 19901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 199 },
                    { 19902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 199 },
                    { 19903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 199 },
                    { 19904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 199 },
                    { 19905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 199 },
                    { 20001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 200 },
                    { 20002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 200 },
                    { 20003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 200 },
                    { 20004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 200 },
                    { 20101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 201 },
                    { 20102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 201 },
                    { 20103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 201 },
                    { 20201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 202 },
                    { 20202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 202 },
                    { 20203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 202 },
                    { 20301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 203 },
                    { 20302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 203 },
                    { 20303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 203 },
                    { 20401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 204 },
                    { 20402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 204 },
                    { 20403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 204 },
                    { 20404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 204 },
                    { 20501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 205 },
                    { 20502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 205 },
                    { 20503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 205 },
                    { 20504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 205 },
                    { 20601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 206 },
                    { 20602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 206 },
                    { 20603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 206 },
                    { 20604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 206 },
                    { 20701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 207 },
                    { 20702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 207 },
                    { 20703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 207 },
                    { 20704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 207 },
                    { 20705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 207 },
                    { 20801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 208 },
                    { 20802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 208 },
                    { 20803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 208 },
                    { 20804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 208 },
                    { 20805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 208 },
                    { 20901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 209 },
                    { 20902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 209 },
                    { 20903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 209 },
                    { 20904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 209 },
                    { 20905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 209 },
                    { 21001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 210 },
                    { 21002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 210 },
                    { 21003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 210 },
                    { 21004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 210 },
                    { 21101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 211 },
                    { 21102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 211 },
                    { 21103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 211 },
                    { 21201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 212 },
                    { 21202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 212 },
                    { 21203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 212 },
                    { 21301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 213 },
                    { 21302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 213 },
                    { 21303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 213 },
                    { 21401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 214 },
                    { 21402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 214 },
                    { 21403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 214 },
                    { 21404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 214 },
                    { 21501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 215 },
                    { 21502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 215 },
                    { 21503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 215 },
                    { 21504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 215 },
                    { 21601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 216 },
                    { 21602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 216 },
                    { 21603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 216 },
                    { 21604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 216 },
                    { 21701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 217 },
                    { 21702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 217 },
                    { 21703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 217 },
                    { 21704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 217 },
                    { 21705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 217 },
                    { 21801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 218 },
                    { 21802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 218 },
                    { 21803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 218 },
                    { 21804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 218 },
                    { 21805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 218 },
                    { 21901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 219 },
                    { 21902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 219 },
                    { 21903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 219 },
                    { 21904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 219 },
                    { 21905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 219 },
                    { 22001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 220 },
                    { 22002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 220 },
                    { 22003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 220 },
                    { 22004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 220 },
                    { 22101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 221 },
                    { 22102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 221 },
                    { 22103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 221 },
                    { 22201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 222 },
                    { 22202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 222 },
                    { 22203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 222 },
                    { 22301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 223 },
                    { 22302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 223 },
                    { 22303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 223 },
                    { 22401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 224 },
                    { 22402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 224 },
                    { 22403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 224 },
                    { 22404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 224 },
                    { 22501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 225 },
                    { 22502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 225 },
                    { 22503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 225 },
                    { 22504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 225 },
                    { 22601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 226 },
                    { 22602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 226 },
                    { 22603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 226 },
                    { 22604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 226 },
                    { 22701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 227 },
                    { 22702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 227 },
                    { 22703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 227 },
                    { 22704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 227 },
                    { 22705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 227 },
                    { 22801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 228 },
                    { 22802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 228 },
                    { 22803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 228 },
                    { 22804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 228 },
                    { 22805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 228 },
                    { 22901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 229 },
                    { 22902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 229 },
                    { 22903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 229 },
                    { 22904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 229 },
                    { 22905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 229 },
                    { 23001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 230 },
                    { 23002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 230 },
                    { 23003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 230 },
                    { 23004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 230 },
                    { 23101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 231 },
                    { 23102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 231 },
                    { 23103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 231 },
                    { 23201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 232 },
                    { 23202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 232 },
                    { 23203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 232 },
                    { 23301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 233 },
                    { 23302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 233 },
                    { 23303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 233 },
                    { 23401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 234 },
                    { 23402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 234 },
                    { 23403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 234 },
                    { 23404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 234 },
                    { 23501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 235 },
                    { 23502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 235 },
                    { 23503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 235 },
                    { 23504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 235 },
                    { 23601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 236 },
                    { 23602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 236 },
                    { 23603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 236 },
                    { 23604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 236 },
                    { 23701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 237 },
                    { 23702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 237 },
                    { 23703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 237 },
                    { 23704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 237 },
                    { 23705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 237 },
                    { 23801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 238 },
                    { 23802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 238 },
                    { 23803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 238 },
                    { 23804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 238 },
                    { 23805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 238 },
                    { 23901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 239 },
                    { 23902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 239 },
                    { 23903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 239 },
                    { 23904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 239 },
                    { 23905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 239 },
                    { 24001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 240 },
                    { 24002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 240 },
                    { 24003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 240 },
                    { 24004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 240 },
                    { 24101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 241 },
                    { 24102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 241 },
                    { 24103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 241 },
                    { 24201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 242 },
                    { 24202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 242 },
                    { 24203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 242 },
                    { 24301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 243 },
                    { 24302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 243 },
                    { 24303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 243 },
                    { 24401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 244 },
                    { 24402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 244 },
                    { 24403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 244 },
                    { 24404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 244 },
                    { 24501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 245 },
                    { 24502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 245 },
                    { 24503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 245 },
                    { 24504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 245 },
                    { 24601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 246 },
                    { 24602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 246 },
                    { 24603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 246 },
                    { 24604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 246 },
                    { 24701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 247 },
                    { 24702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 247 },
                    { 24703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 247 },
                    { 24704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 247 },
                    { 24705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 247 },
                    { 24801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 248 },
                    { 24802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 248 },
                    { 24803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 248 },
                    { 24804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 248 },
                    { 24805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 248 },
                    { 24901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 249 },
                    { 24902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 249 },
                    { 24903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 249 },
                    { 24904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 249 },
                    { 24905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 249 },
                    { 25001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 250 },
                    { 25002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 250 },
                    { 25003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 250 },
                    { 25004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 250 },
                    { 25101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 251 },
                    { 25102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 251 },
                    { 25103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 251 },
                    { 25201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 252 },
                    { 25202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 252 },
                    { 25203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 252 },
                    { 25301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 253 },
                    { 25302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 253 },
                    { 25303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 253 },
                    { 25401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 254 },
                    { 25402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 254 },
                    { 25403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 254 },
                    { 25404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 254 },
                    { 25501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 255 },
                    { 25502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 255 },
                    { 25503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 255 },
                    { 25504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 255 },
                    { 25601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 256 },
                    { 25602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 256 },
                    { 25603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 256 },
                    { 25604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 256 },
                    { 25701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 257 },
                    { 25702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 257 },
                    { 25703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 257 },
                    { 25704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 257 },
                    { 25705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 257 },
                    { 25801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 258 },
                    { 25802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 258 },
                    { 25803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 258 },
                    { 25804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 258 },
                    { 25805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 258 },
                    { 25901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 259 },
                    { 25902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 259 },
                    { 25903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 259 },
                    { 25904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 259 },
                    { 25905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 259 },
                    { 26001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 260 },
                    { 26002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 260 },
                    { 26003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 260 },
                    { 26004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 260 },
                    { 26101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 261 },
                    { 26102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 261 },
                    { 26103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 261 },
                    { 26201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 262 },
                    { 26202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 262 },
                    { 26203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 262 },
                    { 26301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 263 },
                    { 26302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 263 },
                    { 26303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 263 },
                    { 26401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 264 },
                    { 26402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 264 },
                    { 26403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 264 },
                    { 26404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 264 },
                    { 26501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 265 },
                    { 26502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 265 },
                    { 26503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 265 },
                    { 26504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 265 },
                    { 26601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 266 },
                    { 26602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 266 },
                    { 26603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 266 },
                    { 26604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 266 },
                    { 26701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 267 },
                    { 26702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 267 },
                    { 26703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 267 },
                    { 26704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 267 },
                    { 26705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 267 },
                    { 26801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 268 },
                    { 26802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 268 },
                    { 26803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 268 },
                    { 26804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 268 },
                    { 26805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 268 },
                    { 26901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 269 },
                    { 26902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 269 },
                    { 26903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 269 },
                    { 26904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 269 },
                    { 26905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 269 },
                    { 27001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 270 },
                    { 27002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 270 },
                    { 27003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 270 },
                    { 27004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 270 },
                    { 27101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 271 },
                    { 27102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 271 },
                    { 27103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 271 },
                    { 27201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 272 },
                    { 27202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 272 },
                    { 27203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 272 },
                    { 27301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 273 },
                    { 27302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 273 },
                    { 27303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 273 },
                    { 27401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 274 },
                    { 27402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 274 },
                    { 27403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 274 },
                    { 27404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 274 },
                    { 27501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 275 },
                    { 27502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 275 },
                    { 27503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 275 },
                    { 27504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 275 },
                    { 27601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 276 },
                    { 27602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 276 },
                    { 27603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 276 },
                    { 27604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 276 },
                    { 27701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 277 },
                    { 27702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 277 },
                    { 27703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 277 },
                    { 27704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 277 },
                    { 27705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 277 },
                    { 27801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 278 },
                    { 27802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 278 },
                    { 27803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 278 },
                    { 27804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 278 },
                    { 27805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 278 },
                    { 27901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 279 },
                    { 27902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 279 },
                    { 27903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 279 },
                    { 27904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 279 },
                    { 27905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 279 },
                    { 28001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 280 },
                    { 28002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 280 },
                    { 28003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 280 },
                    { 28004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 280 },
                    { 28101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 281 },
                    { 28102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 281 },
                    { 28103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 281 },
                    { 28201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 282 },
                    { 28202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 282 },
                    { 28203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 282 },
                    { 28301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 283 },
                    { 28302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 283 },
                    { 28303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 283 },
                    { 28401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 284 },
                    { 28402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 284 },
                    { 28403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 284 },
                    { 28404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 284 },
                    { 28501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 285 },
                    { 28502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 285 },
                    { 28503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 285 },
                    { 28504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 285 },
                    { 28601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 286 },
                    { 28602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 286 },
                    { 28603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 286 },
                    { 28604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 286 },
                    { 28701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 287 },
                    { 28702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 287 },
                    { 28703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 287 },
                    { 28704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 287 },
                    { 28705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 287 },
                    { 28801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 288 },
                    { 28802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 288 },
                    { 28803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 288 },
                    { 28804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 288 },
                    { 28805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 288 },
                    { 28901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 289 },
                    { 28902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 289 },
                    { 28903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 289 },
                    { 28904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 289 },
                    { 28905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 289 },
                    { 29001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 290 },
                    { 29002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 290 },
                    { 29003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 290 },
                    { 29004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 290 },
                    { 29101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 291 },
                    { 29102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 291 },
                    { 29103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 291 },
                    { 29201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 292 },
                    { 29202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 292 },
                    { 29203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 292 },
                    { 29301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 293 },
                    { 29302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 293 },
                    { 29303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 293 },
                    { 29401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 294 },
                    { 29402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 294 },
                    { 29403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 294 },
                    { 29404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 294 },
                    { 29501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 295 },
                    { 29502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 295 },
                    { 29503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 295 },
                    { 29504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 295 },
                    { 29601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 296 },
                    { 29602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 296 },
                    { 29603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 296 },
                    { 29604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 296 },
                    { 29701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 297 },
                    { 29702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 297 },
                    { 29703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 297 },
                    { 29704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 297 },
                    { 29705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 297 },
                    { 29801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 298 },
                    { 29802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 298 },
                    { 29803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 298 },
                    { 29804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 298 },
                    { 29805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 298 },
                    { 29901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 299 },
                    { 29902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 299 },
                    { 29903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 299 },
                    { 29904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 299 },
                    { 29905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 299 },
                    { 30001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 300 },
                    { 30002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 300 },
                    { 30003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 300 },
                    { 30004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 300 },
                    { 30101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 301 },
                    { 30102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 301 },
                    { 30103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 301 },
                    { 30201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 302 },
                    { 30202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 302 },
                    { 30203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 302 },
                    { 30301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 303 },
                    { 30302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 303 },
                    { 30303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 303 },
                    { 30401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 304 },
                    { 30402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 304 },
                    { 30403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 304 },
                    { 30404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 304 },
                    { 30501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 305 },
                    { 30502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 305 },
                    { 30503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 305 },
                    { 30504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 305 },
                    { 30601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 306 },
                    { 30602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 306 },
                    { 30603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 306 },
                    { 30604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 306 },
                    { 30701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 307 },
                    { 30702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 307 },
                    { 30703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 307 },
                    { 30704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 307 },
                    { 30705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 307 },
                    { 30801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 308 },
                    { 30802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 308 },
                    { 30803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 308 },
                    { 30804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 308 },
                    { 30805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 308 },
                    { 30901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 309 },
                    { 30902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 309 },
                    { 30903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 309 },
                    { 30904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 309 },
                    { 30905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 309 },
                    { 31001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 310 },
                    { 31002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 310 },
                    { 31003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 310 },
                    { 31004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 310 },
                    { 31101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 311 },
                    { 31102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 311 },
                    { 31103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 311 },
                    { 31201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 312 },
                    { 31202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 312 },
                    { 31203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 312 },
                    { 31301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 313 },
                    { 31302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 313 },
                    { 31303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 313 },
                    { 31401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 314 },
                    { 31402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 314 },
                    { 31403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 314 },
                    { 31404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 314 },
                    { 31501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 315 },
                    { 31502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 315 },
                    { 31503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 315 },
                    { 31504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 315 },
                    { 31601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 316 },
                    { 31602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 316 },
                    { 31603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 316 },
                    { 31604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 316 },
                    { 31701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 317 },
                    { 31702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 317 },
                    { 31703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 317 },
                    { 31704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 317 },
                    { 31705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 317 },
                    { 31801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 318 },
                    { 31802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 318 },
                    { 31803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 318 },
                    { 31804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 318 },
                    { 31805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 318 },
                    { 31901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 319 },
                    { 31902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 319 },
                    { 31903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 319 },
                    { 31904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 319 },
                    { 31905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 319 },
                    { 32001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 320 },
                    { 32002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 320 },
                    { 32003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 320 },
                    { 32004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 320 },
                    { 32101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 321 },
                    { 32102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 321 },
                    { 32103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 321 },
                    { 32201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 322 },
                    { 32202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 322 },
                    { 32203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 322 },
                    { 32301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 323 },
                    { 32302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 323 },
                    { 32303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 323 },
                    { 32401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 324 },
                    { 32402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 324 },
                    { 32403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 324 },
                    { 32404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 324 },
                    { 32501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 325 },
                    { 32502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 325 },
                    { 32503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 325 },
                    { 32504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 325 },
                    { 32601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 326 },
                    { 32602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 326 },
                    { 32603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 326 },
                    { 32604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 326 },
                    { 32701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 327 },
                    { 32702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 327 },
                    { 32703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 327 },
                    { 32704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 327 },
                    { 32705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 327 },
                    { 32801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 328 },
                    { 32802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 328 },
                    { 32803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 328 },
                    { 32804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 328 },
                    { 32805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 328 },
                    { 32901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 329 },
                    { 32902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 329 },
                    { 32903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 329 },
                    { 32904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 329 },
                    { 32905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 329 },
                    { 33001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 330 },
                    { 33002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 330 },
                    { 33003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 330 },
                    { 33004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 330 },
                    { 33101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 331 },
                    { 33102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 331 },
                    { 33103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 331 },
                    { 33201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 332 },
                    { 33202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 332 },
                    { 33203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 332 },
                    { 33301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 333 },
                    { 33302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 333 },
                    { 33303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 333 },
                    { 33401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 334 },
                    { 33402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 334 },
                    { 33403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 334 },
                    { 33404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 334 },
                    { 33501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 335 },
                    { 33502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 335 },
                    { 33503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 335 },
                    { 33504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 335 },
                    { 33601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 336 },
                    { 33602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 336 },
                    { 33603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 336 },
                    { 33604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 336 },
                    { 33701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 337 },
                    { 33702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 337 },
                    { 33703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 337 },
                    { 33704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 337 },
                    { 33705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 337 },
                    { 33801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 338 },
                    { 33802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 338 },
                    { 33803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 338 },
                    { 33804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 338 },
                    { 33805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 338 },
                    { 33901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 339 },
                    { 33902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 339 },
                    { 33903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 339 },
                    { 33904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 339 },
                    { 33905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 339 },
                    { 34001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 340 },
                    { 34002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 340 },
                    { 34003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 340 },
                    { 34004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 340 },
                    { 34101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 341 },
                    { 34102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 341 },
                    { 34103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 341 },
                    { 34201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 342 },
                    { 34202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 342 },
                    { 34203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 342 },
                    { 34301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 343 },
                    { 34302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 343 },
                    { 34303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 343 },
                    { 34401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 344 },
                    { 34402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 344 },
                    { 34403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 344 },
                    { 34404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 344 },
                    { 34501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 345 },
                    { 34502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 345 },
                    { 34503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 345 },
                    { 34504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 345 },
                    { 34601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 346 },
                    { 34602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 346 },
                    { 34603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 346 },
                    { 34604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 346 },
                    { 34701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 347 },
                    { 34702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 347 },
                    { 34703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 347 },
                    { 34704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 347 },
                    { 34705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 347 },
                    { 34801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 348 },
                    { 34802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 348 },
                    { 34803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 348 },
                    { 34804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 348 },
                    { 34805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 348 },
                    { 34901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 349 },
                    { 34902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 349 },
                    { 34903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 349 },
                    { 34904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 349 },
                    { 34905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 349 },
                    { 35001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 350 },
                    { 35002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 350 },
                    { 35003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 350 },
                    { 35004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 350 },
                    { 35101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 351 },
                    { 35102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 351 },
                    { 35103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 351 },
                    { 35201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 352 },
                    { 35202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 352 },
                    { 35203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 352 },
                    { 35301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 353 },
                    { 35302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 353 },
                    { 35303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 353 },
                    { 35401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 354 },
                    { 35402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 354 },
                    { 35403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 354 },
                    { 35404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 354 },
                    { 35501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 355 },
                    { 35502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 355 },
                    { 35503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 355 },
                    { 35504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 355 },
                    { 35601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 356 },
                    { 35602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 356 },
                    { 35603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 356 },
                    { 35604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 356 },
                    { 35701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 357 },
                    { 35702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 357 },
                    { 35703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 357 },
                    { 35704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 357 },
                    { 35705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 357 },
                    { 35801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 358 },
                    { 35802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 358 },
                    { 35803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 358 },
                    { 35804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 358 },
                    { 35805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 358 },
                    { 35901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 359 },
                    { 35902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 359 },
                    { 35903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 359 },
                    { 35904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 359 },
                    { 35905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 359 },
                    { 36001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 360 },
                    { 36002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 360 },
                    { 36003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 360 },
                    { 36004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 360 },
                    { 36101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 361 },
                    { 36102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 361 },
                    { 36103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 361 },
                    { 36201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 362 },
                    { 36202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 362 },
                    { 36203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 362 },
                    { 36301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 363 },
                    { 36302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 363 },
                    { 36303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 363 },
                    { 36401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 364 },
                    { 36402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 364 },
                    { 36403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 364 },
                    { 36404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 364 },
                    { 36501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 365 },
                    { 36502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 365 },
                    { 36503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 365 },
                    { 36504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 365 },
                    { 36601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 366 },
                    { 36602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 366 },
                    { 36603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 366 },
                    { 36604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 366 },
                    { 36701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 367 },
                    { 36702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 367 },
                    { 36703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 367 },
                    { 36704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 367 },
                    { 36705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 367 },
                    { 36801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 368 },
                    { 36802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 368 },
                    { 36803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 368 },
                    { 36804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 368 },
                    { 36805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 368 },
                    { 36901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 369 },
                    { 36902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 369 },
                    { 36903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 369 },
                    { 36904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 369 },
                    { 36905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 369 },
                    { 37001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 370 },
                    { 37002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 370 },
                    { 37003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 370 },
                    { 37004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 370 },
                    { 37101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 371 },
                    { 37102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 371 },
                    { 37103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 371 },
                    { 37201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 372 },
                    { 37202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 372 },
                    { 37203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 372 },
                    { 37301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 373 },
                    { 37302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 373 },
                    { 37303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 373 },
                    { 37401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 374 },
                    { 37402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 374 },
                    { 37403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 374 },
                    { 37404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 374 },
                    { 37501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 375 },
                    { 37502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 375 },
                    { 37503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 375 },
                    { 37504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 375 },
                    { 37601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 376 },
                    { 37602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 376 },
                    { 37603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 376 },
                    { 37604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 376 },
                    { 37701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 377 },
                    { 37702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 377 },
                    { 37703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 377 },
                    { 37704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 377 },
                    { 37705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 377 },
                    { 37801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 378 },
                    { 37802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 378 },
                    { 37803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 378 },
                    { 37804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 378 },
                    { 37805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 378 },
                    { 37901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 379 },
                    { 37902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 379 },
                    { 37903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 379 },
                    { 37904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 379 },
                    { 37905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 379 },
                    { 38001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 380 },
                    { 38002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 380 },
                    { 38003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 380 },
                    { 38004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 380 },
                    { 38101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 381 },
                    { 38102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 381 },
                    { 38103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 381 },
                    { 38201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 382 },
                    { 38202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 382 },
                    { 38203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 382 },
                    { 38301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 383 },
                    { 38302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 383 },
                    { 38303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 383 },
                    { 38401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 384 },
                    { 38402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 384 },
                    { 38403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 384 },
                    { 38404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 384 },
                    { 38501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 385 },
                    { 38502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 385 },
                    { 38503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 385 },
                    { 38504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 385 },
                    { 38601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 386 },
                    { 38602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 386 },
                    { 38603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 386 },
                    { 38604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 386 },
                    { 38701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 387 },
                    { 38702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 387 },
                    { 38703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 387 },
                    { 38704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 387 },
                    { 38705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 387 },
                    { 38801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 388 },
                    { 38802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 388 },
                    { 38803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 388 },
                    { 38804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 388 },
                    { 38805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 388 },
                    { 38901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 389 },
                    { 38902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 389 },
                    { 38903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 389 },
                    { 38904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 389 },
                    { 38905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 389 },
                    { 39001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 390 },
                    { 39002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 390 },
                    { 39003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 390 },
                    { 39004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 390 },
                    { 39101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 391 },
                    { 39102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 391 },
                    { 39103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 391 },
                    { 39201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 392 },
                    { 39202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 392 },
                    { 39203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 392 },
                    { 39301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 393 },
                    { 39302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 393 },
                    { 39303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 393 },
                    { 39401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 394 },
                    { 39402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 394 },
                    { 39403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 394 },
                    { 39404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 394 },
                    { 39501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 395 },
                    { 39502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 395 },
                    { 39503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 395 },
                    { 39504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 395 },
                    { 39601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 396 },
                    { 39602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 396 },
                    { 39603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 396 },
                    { 39604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 396 },
                    { 39701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 397 },
                    { 39702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 397 },
                    { 39703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 397 },
                    { 39704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 397 },
                    { 39705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 397 },
                    { 39801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 398 },
                    { 39802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 398 },
                    { 39803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 398 },
                    { 39804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 398 },
                    { 39805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 398 },
                    { 39901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 399 },
                    { 39902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 399 },
                    { 39903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 399 },
                    { 39904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 399 },
                    { 39905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 399 },
                    { 40001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 400 },
                    { 40002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 400 },
                    { 40003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 400 },
                    { 40004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 400 },
                    { 40101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 401 },
                    { 40102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 401 },
                    { 40103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 401 },
                    { 40201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 402 },
                    { 40202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 402 },
                    { 40203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 402 },
                    { 40301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 403 },
                    { 40302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 403 },
                    { 40303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 403 },
                    { 40401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 404 },
                    { 40402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 404 },
                    { 40403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 404 },
                    { 40404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 404 },
                    { 40501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 405 },
                    { 40502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 405 },
                    { 40503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 405 },
                    { 40504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 405 },
                    { 40601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 406 },
                    { 40602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 406 },
                    { 40603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 406 },
                    { 40604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 406 },
                    { 40701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 407 },
                    { 40702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 407 },
                    { 40703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 407 },
                    { 40704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 407 },
                    { 40705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 407 },
                    { 40801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 408 },
                    { 40802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 408 },
                    { 40803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 408 },
                    { 40804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 408 },
                    { 40805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 408 },
                    { 40901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 409 },
                    { 40902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 409 },
                    { 40903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 409 },
                    { 40904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 409 },
                    { 40905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 409 },
                    { 41001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 410 },
                    { 41002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 410 },
                    { 41003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 410 },
                    { 41004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 410 },
                    { 41101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 411 },
                    { 41102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 411 },
                    { 41103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 411 },
                    { 41201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 412 },
                    { 41202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 412 },
                    { 41203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 412 },
                    { 41301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 413 },
                    { 41302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 413 },
                    { 41303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 413 },
                    { 41401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 414 },
                    { 41402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 414 },
                    { 41403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 414 },
                    { 41404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 414 },
                    { 41501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 415 },
                    { 41502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 415 },
                    { 41503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 415 },
                    { 41504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 415 },
                    { 41601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 416 },
                    { 41602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 416 },
                    { 41603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 416 },
                    { 41604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 416 },
                    { 41701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 417 },
                    { 41702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 417 },
                    { 41703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 417 },
                    { 41704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 417 },
                    { 41705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 417 },
                    { 41801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 418 },
                    { 41802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 418 },
                    { 41803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 418 },
                    { 41804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 418 },
                    { 41805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 418 },
                    { 41901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 419 },
                    { 41902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 419 },
                    { 41903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 419 },
                    { 41904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 419 },
                    { 41905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 419 },
                    { 42001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 420 },
                    { 42002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 420 },
                    { 42003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 420 },
                    { 42004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 420 },
                    { 42101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 421 },
                    { 42102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 421 },
                    { 42103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 421 },
                    { 42201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 422 },
                    { 42202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 422 },
                    { 42203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 422 },
                    { 42301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 423 },
                    { 42302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 423 },
                    { 42303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 423 },
                    { 42401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 424 },
                    { 42402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 424 },
                    { 42403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 424 },
                    { 42404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 424 },
                    { 42501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 425 },
                    { 42502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 425 },
                    { 42503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 425 },
                    { 42504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 425 },
                    { 42601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 426 },
                    { 42602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 426 },
                    { 42603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 426 },
                    { 42604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 426 },
                    { 42701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 427 },
                    { 42702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 427 },
                    { 42703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 427 },
                    { 42704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 427 },
                    { 42705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 427 },
                    { 42801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 428 },
                    { 42802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 428 },
                    { 42803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 428 },
                    { 42804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 428 },
                    { 42805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 428 },
                    { 42901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 429 },
                    { 42902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 429 },
                    { 42903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 429 },
                    { 42904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 429 },
                    { 42905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 429 },
                    { 43001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 430 },
                    { 43002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 430 },
                    { 43003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 430 },
                    { 43004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 430 },
                    { 43101, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 431 },
                    { 43102, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 431 },
                    { 43103, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 431 },
                    { 43201, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 432 },
                    { 43202, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 432 },
                    { 43203, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 432 },
                    { 43301, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 433 },
                    { 43302, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 433 },
                    { 43303, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 433 },
                    { 43401, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 434 },
                    { 43402, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 434 },
                    { 43403, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 434 },
                    { 43404, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 434 },
                    { 43501, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 435 },
                    { 43502, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 435 },
                    { 43503, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 435 },
                    { 43504, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 435 },
                    { 43601, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 436 },
                    { 43602, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 436 },
                    { 43603, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 436 },
                    { 43604, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 4, 436 },
                    { 43701, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 437 },
                    { 43702, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 437 },
                    { 43703, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 437 },
                    { 43704, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 437 },
                    { 43705, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 437 },
                    { 43801, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 438 },
                    { 43802, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 438 },
                    { 43803, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 438 },
                    { 43804, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 438 },
                    { 43805, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 5, 438 },
                    { 43901, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 439 },
                    { 43902, new Guid("33333333-0000-4000-8000-000000000202"), 1.0, false, 2, 439 },
                    { 43903, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 439 },
                    { 43904, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 439 },
                    { 43905, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 5, 439 },
                    { 44001, new Guid("33333333-0000-4000-8000-000000000201"), 1.0, false, 1, 440 },
                    { 44002, new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true, 2, 440 },
                    { 44003, new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false, 3, 440 },
                    { 44004, new Guid("33333333-0000-4000-8000-000000000204"), 1.0, false, 4, 440 },
                    { 44101, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 441 },
                    { 44102, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 441 },
                    { 44103, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 441 },
                    { 44201, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 442 },
                    { 44202, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 442 },
                    { 44203, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 442 },
                    { 44301, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 443 },
                    { 44302, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 443 },
                    { 44303, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 443 },
                    { 44401, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 444 },
                    { 44402, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 444 },
                    { 44403, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 444 },
                    { 44404, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 444 },
                    { 44501, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 445 },
                    { 44502, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 445 },
                    { 44503, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 445 },
                    { 44504, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 445 },
                    { 44601, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 446 },
                    { 44602, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 446 },
                    { 44603, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 446 },
                    { 44604, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 4, 446 },
                    { 44701, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 447 },
                    { 44702, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 447 },
                    { 44703, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 447 },
                    { 44704, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 447 },
                    { 44705, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 447 },
                    { 44801, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 448 },
                    { 44802, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 448 },
                    { 44803, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 448 },
                    { 44804, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 448 },
                    { 44805, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 5, 448 },
                    { 44901, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 449 },
                    { 44902, new Guid("33333333-0000-4000-8000-000000000212"), 1.0, false, 2, 449 },
                    { 44903, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 449 },
                    { 44904, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 449 },
                    { 44905, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 5, 449 },
                    { 45001, new Guid("33333333-0000-4000-8000-000000000211"), 1.0, false, 1, 450 },
                    { 45002, new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true, 2, 450 },
                    { 45003, new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false, 3, 450 },
                    { 45004, new Guid("33333333-0000-4000-8000-000000000214"), 1.0, false, 4, 450 },
                    { 45101, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 451 },
                    { 45102, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 451 },
                    { 45103, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 451 },
                    { 45201, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 452 },
                    { 45202, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 452 },
                    { 45203, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 452 },
                    { 45301, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 453 },
                    { 45302, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 453 },
                    { 45303, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 453 },
                    { 45401, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 454 },
                    { 45402, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 454 },
                    { 45403, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 454 },
                    { 45404, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 454 },
                    { 45501, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 455 },
                    { 45502, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 455 },
                    { 45503, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 455 },
                    { 45504, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 455 },
                    { 45601, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 456 },
                    { 45602, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 456 },
                    { 45603, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 456 },
                    { 45604, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 4, 456 },
                    { 45701, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 457 },
                    { 45702, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 457 },
                    { 45703, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 457 },
                    { 45704, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 457 },
                    { 45705, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 457 },
                    { 45801, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 458 },
                    { 45802, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 458 },
                    { 45803, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 458 },
                    { 45804, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 458 },
                    { 45805, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 5, 458 },
                    { 45901, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 459 },
                    { 45902, new Guid("33333333-0000-4000-8000-000000000222"), 1.0, false, 2, 459 },
                    { 45903, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 459 },
                    { 45904, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 459 },
                    { 45905, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 5, 459 },
                    { 46001, new Guid("33333333-0000-4000-8000-000000000221"), 1.0, false, 1, 460 },
                    { 46002, new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true, 2, 460 },
                    { 46003, new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false, 3, 460 },
                    { 46004, new Guid("33333333-0000-4000-8000-000000000224"), 1.0, false, 4, 460 },
                    { 46101, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 461 },
                    { 46102, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 461 },
                    { 46103, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 461 },
                    { 46201, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 462 },
                    { 46202, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 462 },
                    { 46203, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 462 },
                    { 46301, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 463 },
                    { 46302, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 463 },
                    { 46303, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 463 },
                    { 46401, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 464 },
                    { 46402, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 464 },
                    { 46403, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 464 },
                    { 46404, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 464 },
                    { 46501, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 465 },
                    { 46502, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 465 },
                    { 46503, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 465 },
                    { 46504, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 465 },
                    { 46601, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 466 },
                    { 46602, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 466 },
                    { 46603, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 466 },
                    { 46604, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 4, 466 },
                    { 46701, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 467 },
                    { 46702, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 467 },
                    { 46703, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 467 },
                    { 46704, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 467 },
                    { 46705, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 467 },
                    { 46801, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 468 },
                    { 46802, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 468 },
                    { 46803, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 468 },
                    { 46804, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 468 },
                    { 46805, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 5, 468 },
                    { 46901, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 469 },
                    { 46902, new Guid("33333333-0000-4000-8000-000000000232"), 1.0, false, 2, 469 },
                    { 46903, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 469 },
                    { 46904, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 469 },
                    { 46905, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 5, 469 },
                    { 47001, new Guid("33333333-0000-4000-8000-000000000231"), 1.0, false, 1, 470 },
                    { 47002, new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true, 2, 470 },
                    { 47003, new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false, 3, 470 },
                    { 47004, new Guid("33333333-0000-4000-8000-000000000234"), 1.0, false, 4, 470 },
                    { 47101, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 471 },
                    { 47102, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 471 },
                    { 47103, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 471 },
                    { 47201, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 472 },
                    { 47202, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 472 },
                    { 47203, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 472 },
                    { 47301, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 473 },
                    { 47302, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 473 },
                    { 47303, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 473 },
                    { 47401, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 474 },
                    { 47402, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 474 },
                    { 47403, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 474 },
                    { 47404, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 474 },
                    { 47501, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 475 },
                    { 47502, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 475 },
                    { 47503, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 475 },
                    { 47504, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 475 },
                    { 47601, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 476 },
                    { 47602, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 476 },
                    { 47603, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 476 },
                    { 47604, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 4, 476 },
                    { 47701, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 477 },
                    { 47702, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 477 },
                    { 47703, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 477 },
                    { 47704, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 477 },
                    { 47705, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 477 },
                    { 47801, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 478 },
                    { 47802, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 478 },
                    { 47803, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 478 },
                    { 47804, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 478 },
                    { 47805, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 5, 478 },
                    { 47901, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 479 },
                    { 47902, new Guid("33333333-0000-4000-8000-000000000242"), 1.0, false, 2, 479 },
                    { 47903, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 479 },
                    { 47904, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 479 },
                    { 47905, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 5, 479 },
                    { 48001, new Guid("33333333-0000-4000-8000-000000000241"), 1.0, false, 1, 480 },
                    { 48002, new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true, 2, 480 },
                    { 48003, new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false, 3, 480 },
                    { 48004, new Guid("33333333-0000-4000-8000-000000000244"), 1.0, false, 4, 480 },
                    { 48101, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 481 },
                    { 48102, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 481 },
                    { 48103, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 481 },
                    { 48201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 482 },
                    { 48202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 482 },
                    { 48203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 482 },
                    { 48301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 483 },
                    { 48302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 483 },
                    { 48303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 483 },
                    { 48401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 484 },
                    { 48402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 484 },
                    { 48403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 484 },
                    { 48404, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 484 },
                    { 48501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 485 },
                    { 48502, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 485 },
                    { 48503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 485 },
                    { 48504, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 485 },
                    { 48601, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 486 },
                    { 48602, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 486 },
                    { 48603, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 486 },
                    { 48604, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 486 },
                    { 48701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 487 },
                    { 48702, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 487 },
                    { 48703, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 487 },
                    { 48704, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 487 },
                    { 48705, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 487 },
                    { 48801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 488 },
                    { 48802, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 488 },
                    { 48803, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 488 },
                    { 48804, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 488 },
                    { 48805, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 5, 488 },
                    { 48901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 489 },
                    { 48902, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 489 },
                    { 48903, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 489 },
                    { 48904, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 489 },
                    { 48905, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 489 },
                    { 49001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 490 },
                    { 49002, new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true, 2, 490 },
                    { 49003, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 490 },
                    { 49004, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 490 },
                    { 49101, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 491 },
                    { 49102, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 491 },
                    { 49103, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 491 },
                    { 49201, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 492 },
                    { 49202, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 492 },
                    { 49203, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 492 },
                    { 49301, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 493 },
                    { 49302, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 493 },
                    { 49303, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 493 },
                    { 49401, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 494 },
                    { 49402, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 494 },
                    { 49403, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 494 },
                    { 49404, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 494 },
                    { 49501, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 495 },
                    { 49502, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 495 },
                    { 49503, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 495 },
                    { 49504, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 495 },
                    { 49601, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 496 },
                    { 49602, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 496 },
                    { 49603, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 496 },
                    { 49604, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 4, 496 },
                    { 49701, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 497 },
                    { 49702, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 497 },
                    { 49703, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 497 },
                    { 49704, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 497 },
                    { 49705, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 497 },
                    { 49801, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 498 },
                    { 49802, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 498 },
                    { 49803, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 498 },
                    { 49804, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 498 },
                    { 49805, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 5, 498 },
                    { 49901, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 499 },
                    { 49902, new Guid("33333333-0000-4000-8000-000000000262"), 1.0, false, 2, 499 },
                    { 49903, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 499 },
                    { 49904, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 499 },
                    { 49905, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 5, 499 },
                    { 50001, new Guid("33333333-0000-4000-8000-000000000261"), 1.0, false, 1, 500 },
                    { 50002, new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true, 2, 500 },
                    { 50003, new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false, 3, 500 },
                    { 50004, new Guid("33333333-0000-4000-8000-000000000264"), 1.0, false, 4, 500 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49004);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49101);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49102);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49103);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49201);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49202);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49203);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49301);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49302);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49303);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49401);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49402);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49403);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49404);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49501);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49502);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49503);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49504);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49601);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49602);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49603);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49701);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49702);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49703);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49704);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49705);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49801);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49802);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49803);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49804);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49805);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49901);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49902);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49903);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49904);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49905);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50001);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50002);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50003);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50004);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 150);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 151);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 152);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 153);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 154);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 155);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 156);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 157);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 161);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 162);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 163);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 167);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 168);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 169);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 170);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 171);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 172);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 173);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 174);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 175);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 176);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 177);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 178);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 179);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 180);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 181);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 182);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 183);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 184);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 185);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 186);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 187);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 188);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 189);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 190);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 191);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 192);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 193);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 194);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 195);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 196);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 197);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 198);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 199);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 214);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 215);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 216);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 217);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 218);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 219);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 220);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 221);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 222);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 223);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 224);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 225);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 226);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 227);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 228);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 229);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 230);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 231);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 232);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 233);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 234);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 235);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 236);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 237);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 238);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 239);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 240);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 241);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 242);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 243);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 244);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 245);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 246);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 247);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 248);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 249);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 250);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 251);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 252);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 253);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 254);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 256);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 257);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 258);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 259);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 260);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 261);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 262);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 263);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 264);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 265);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 266);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 267);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 268);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 269);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 270);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 271);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 272);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 273);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 274);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 275);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 276);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 277);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 278);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 279);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 280);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 281);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 282);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 283);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 284);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 285);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 286);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 287);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 288);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 289);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 290);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 291);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 292);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 293);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 294);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 295);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 296);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 297);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 298);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 299);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 304);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 305);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 306);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 308);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 309);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 310);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 311);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 312);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 313);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 314);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 315);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 316);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 317);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 318);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 319);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 326);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 327);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 328);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 329);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 330);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 331);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 332);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 333);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 334);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 335);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 336);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 337);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 338);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 339);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 340);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 341);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 342);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 343);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 344);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 345);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 346);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 347);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 348);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 349);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 350);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 351);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 352);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 353);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 354);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 355);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 356);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 357);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 358);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 359);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 360);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 361);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 362);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 363);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 364);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 365);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 366);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 367);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 368);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 369);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 370);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 371);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 372);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 373);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 374);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 375);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 376);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 377);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 378);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 379);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 380);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 381);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 382);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 383);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 384);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 385);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 386);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 387);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 388);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 389);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 390);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 391);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 392);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 393);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 394);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 395);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 396);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 397);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 398);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 399);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 400);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 405);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 406);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 407);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 408);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 409);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 410);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 411);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 412);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 413);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 414);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 415);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 416);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 417);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 418);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 419);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 420);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 421);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 422);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 423);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 424);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 425);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 426);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 427);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 428);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 429);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 430);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 431);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 432);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 433);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 434);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 435);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 436);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 437);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 438);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 439);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 440);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 441);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 442);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 443);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 444);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 445);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 446);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 447);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 448);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 449);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 450);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 451);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 452);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 453);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 454);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 455);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 456);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 457);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 458);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 459);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 460);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 461);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 462);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 463);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 464);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 465);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 466);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 467);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 468);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 469);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 470);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 471);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 472);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 473);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 474);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 475);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 476);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 477);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 478);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 479);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 480);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 481);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 482);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 483);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 484);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 485);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 486);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 487);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 488);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 489);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 490);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 491);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 492);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 493);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 494);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 495);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 496);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 497);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 498);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 499);

            migrationBuilder.DeleteData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 500);

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000201"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000202"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000203"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000204"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000205"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000211"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000212"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000213"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000214"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000215"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000221"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000222"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000223"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000224"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000225"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000231"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000232"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000233"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000234"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000235"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000241"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000242"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000243"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000244"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000245"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000255"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000261"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000262"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000263"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000264"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000265"));

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "An orc leads scouts through the smouldering tunnels.");

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "A skeleton mage has joined the orcs in the hall of ash.");

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "The orc warlord's camp. His aura sets everything ablaze.");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Name", "UnitType" },
                values: new object[] { "Orc Brute", "Orc" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Name", "UnitType" },
                values: new object[] { "Orc Warlord", "Orc" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "UnitType",
                value: "Orc");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                column: "UnitType",
                value: "Troll");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                column: "UnitType",
                value: "Yeti");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                column: "UnitType",
                value: "Orc");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                column: "UnitType",
                value: "Ogre");
        }
    }
}
