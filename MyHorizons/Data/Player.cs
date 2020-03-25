﻿using MyHorizons.Data.Save;
using MyHorizons.Encryption;

namespace MyHorizons.Data
{
    public sealed class Player
    {
        public readonly int Index;

        public string Name;
        public uint PlayerUID;
        public string TownName;
        public uint TownUID;
        public Item[] Pockets;
        public EncryptedInt32 Wallet;
        public EncryptedInt32 Bank;
        public EncryptedInt32 NookMiles;

        private readonly PersonalSaveFile _personalFile;

        private readonly struct Offsets
        {
            public readonly int PersonalId;
            public readonly int Pockets;
            public readonly int Wallet;
            public readonly int Bank;
            public readonly int NookMiles;
            public readonly int Photo;

            public Offsets(int pid, int pockets, int wallet, int bank, int nookMiles, int photo)
            {
                PersonalId = pid;
                Pockets = pockets;
                Wallet = wallet;
                Bank = bank;
                NookMiles = nookMiles;
                Photo = photo;
            }
        }

        private static readonly Offsets[] PlayerOffsetsByRevision =
        {
            new Offsets(0xB0A0, 0x35BD4, 0x11578, 0x68BE4, 0x11570, 0x11598),
            new Offsets(0xB0B8, 0x35C20, 0x11590, 0x68C34, 0x11588, 0x115C4)
        };

        private static Offsets GetOffsetsFromRevision() => PlayerOffsetsByRevision[MainSaveFile.Singleton().GetRevision()];

        public Player(int idx, PersonalSaveFile personalSave)
        {
            _personalFile = personalSave;
            var offsets = GetOffsetsFromRevision();
            Index = idx;
            // TODO: Convert this to a "PersonalID" struct
            TownUID = personalSave.ReadU32(offsets.PersonalId);
            TownName = personalSave.ReadString(offsets.PersonalId + 4, 10);
            PlayerUID = personalSave.ReadU32(offsets.PersonalId + 0x1C);
            Name = personalSave.ReadString(offsets.PersonalId + 0x20, 10);

            Wallet = new EncryptedInt32(personalSave, offsets.Wallet);
            Bank = new EncryptedInt32(personalSave, offsets.Bank);
            NookMiles = new EncryptedInt32(personalSave, offsets.NookMiles);
        }

        public void Save()
        {
            var offsets = GetOffsetsFromRevision();
            _personalFile.WriteU32(offsets.PersonalId, TownUID);
            _personalFile.WriteString(offsets.PersonalId + 4, TownName, 10);
            _personalFile.WriteU32(offsets.PersonalId + 0x1C, PlayerUID);
            _personalFile.WriteString(offsets.PersonalId + 0x20, Name, 10);

            Wallet.Write(_personalFile, offsets.Wallet);
            Bank.Write(_personalFile, offsets.Bank);
            NookMiles.Write(_personalFile, offsets.NookMiles);
        }

        public byte[] GetPhotoData()
        {
            var offset = GetOffsetsFromRevision().Photo;
            if (_personalFile.ReadU16(offset) != 0xD8FF)
                return null;
            // TODO: Determine actual size buffer instead of using this.
            var size = 2;
            while (_personalFile.ReadU16(offset + size) != 0xD9FF)
                size++;
            return _personalFile.ReadArray<byte>(offset, size + 2);
        }
    }
}
