# 📦 Supplier

An [InfiniteChests](https://github.com/MarioFoli/InfiniteChestsV3) successor. 
We do things better, faster, and easier. Supplier allows admins to create infinite chests which restock their items instantly. Simply create a chest, put your items in and use /infchest add. Boom! You're done :)

# So, how is this better than [InfChests V3](https://github.com/MarioFoli/InfiniteChestsV3)?
 - Mobile-crossplay support
 - No more corrupting chests, actual in-game chests are completely untouched
    - If the plugin is removed, the chest will continue working, but no longer be infinite
 - Items regenerate while you are in the chest. That's right, you don't have to get out and open the chest again to see the items come back. They get restocked in real-time!
 - Boilerplate reduction -> instead of typing in three or more commands (/chest claim, /chest public, /chest refill), it's been reduced to one: /infchest add
 - Uses MongoDB -> much faster
 - Modern, it was written natively for TShock V5, and .NET 6
 - Low overhead, it's quick, tiny (file size is 16 KB), and has very little impact on performance
 - Actively supported, by me.

## ⚠️ Dependencies:

Supplier releases are already bundled with all of the dependencies required **HOWEVER, an active & working MongoDB is required for Auxiliary to function properly,** which Supplier depends on.
- CSF.Net.TShock
- Auxiliary

## 📜 Commands List:

| Command        |Description     |Permission    |
| ------------- |:-------------:|  :-----------:|
| /infchest add    |Adds the selected chest as an infinite chest | tbc.admin |
| /infchest del    |Deletes the selected infinite chest and reverts it to normal | tbc.admin |
| /cancel    |Cancel's the infchest selection | tbc.admin |
