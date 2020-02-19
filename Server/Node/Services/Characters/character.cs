/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Text.RegularExpressions;
using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class character : Service
    {
        enum NameValidationResults
        {
            Valid = 1,
            TooShort = -1,
            TooLong = -2,
            IllegalCharacters = -5,
            MoreThanOneSpace = -6,
            Taken = -101,
            Banned = -102
        };

        private readonly CharacterDB mDB = null;
        private readonly CacheStorage mCacheStorage = null;

        public character(CacheStorage cacheStorage, DatabaseConnection db)
            : base("character")
        {
            this.mDB = new CharacterDB(db);
            this.mCacheStorage = cacheStorage;
        }

        public PyDataType GetCharactersToSelect(PyTuple args, Client client)
        {
            return this.mDB.GetCharacterList(client.AccountID);
        }

        public PyDataType LogStartOfCharacterCreation(PyTuple args, Client client)
        {
            return null;
        }

        public PyDataType GetCharCreationInfo(PyTuple args, Client client)
        {
            return this.mCacheStorage.GetHints(CacheStorage.CreateCharacterCacheTable);
        }

        public PyDataType GetAppearanceInfo(PyTuple args, Client client)
        {
            return this.mCacheStorage.GetHints(CacheStorage.CharacterAppearanceCacheTable);
        }

        public PyDataType GetCharNewExtraCreationInfo(PyTuple args, Client client)
        {
            return new PyDictionary();
        }

        public PyInteger ValidateNameEx(PyTuple args, Client client)
        {
            // TODO: IMPROVE PARSING OF PACKETS LIKE THIS
            if (args.Count != 1)
                throw new Exception($"Expected tuple of size 1 but got {args.Count}");

            string characterName = args[0] as PyString;

            if (characterName.Length < 3)
                return new PyInteger((int) NameValidationResults.TooShort);

            // equivalent to the itemName column in the entity table
            if (characterName.Length > 85)
                return new PyInteger((int) NameValidationResults.TooLong);

            // ensure only alphanumeric characters and/or spaces are used
            if (Regex.IsMatch(characterName, "^[a-zA-Z0-9 ]*$") == false)
                return new PyInteger((int) NameValidationResults.IllegalCharacters);

            if (characterName.IndexOf(' ') != characterName.LastIndexOf(' '))
                return new PyInteger((int) NameValidationResults.MoreThanOneSpace);

            if (this.mDB.IsCharacterNameTaken(characterName) == true)
                return new PyInteger((int) NameValidationResults.Taken);

            // TODO: IMPLEMENT BANLIST OF WORDS
            return new PyInteger((int) NameValidationResults.Valid);
        }

        public PyDataType CreateCharacter2(PyTuple arguments, Client client)
        {
            if(this.ValidateNameEx (new PyTuple(new PyDataType [] { arguments[0] }), client) != (int) NameValidationResults.Valid)
                throw new UserError("CharNameInvalid");

            throw new UserError("CharNameInvalid");
        }
    }
}