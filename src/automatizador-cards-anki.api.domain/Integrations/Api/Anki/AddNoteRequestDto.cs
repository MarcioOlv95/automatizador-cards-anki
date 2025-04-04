﻿namespace automatizador_cards_anki.api.domain.Integrations.Api.Anki;

public class AddNoteRequestDto
{
    public string action { get; set; }
    public int version { get; set; }
    public Params @params { get; set; } = new();

    public AddNoteRequestDto(string action, int version)
    {
        this.action = action;
        this.version = version;
    }
}

public class Params
{
    public List<Note> notes { get; set; } = [];
}

public class Note
{
    public string deckName { get; set; }
    public string modelName { get; set; }
    public Field fields { get; set; }
    public Option options { get; set; } = new();
    public List<string> Tags { get; set; } = [];
    public MediaFile Audio { get; set; }
    public List<MediaFile> picture { get; set; } = [];

    public Note(string deckName, string modelName, Field fields)
    {
        this.deckName = deckName;
        this.modelName = modelName;
        this.fields = fields;
    }
}

public class Field
{
    public string Front { get; set; }
    public string Back { get; set; }

    public Field(string front, string back)
    {
        Front = front;
        Back = back;
    }
}

public class Option
{
    public bool allowDuplicate { get; set; }
    public string duplicateScope { get; set; } = string.Empty;
    public DuplicateScopeOption duplicateScopeOptions { get; set; } = new();
}

public class DuplicateScopeOption
{
    public string deckName { get; set; } = string.Empty;
    public bool checkChildren { get; set; }
    public bool checkAllModels { get; set; }
}

public class MediaFile
{
    public string path { get; set; } = string.Empty;
    public string filename { get; set; } = string.Empty;
    public List<string> fields { get; set; } = [];

    public MediaFile(string path, string fileName, List<string> fields)
    {
        this.path = path;
        this.filename = fileName;
        this.fields = fields;
    }

    public MediaFile(string path, List<string> fields)
    {
        this.path = path;
        this.fields = fields;
    }
}