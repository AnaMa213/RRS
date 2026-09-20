using UnityEditor;

/// <summary>
/// Story 5.16 : force l'import des sources modifiees hors focus de l'Editeur. La base d'assets ne
/// detecte pas les ecritures faites directement sur disque tant qu'aucun Refresh n'a eu lieu, ce qui
/// fait dire a 'recompile' qu'aucun script n'a besoin d'etre recompile alors que les assemblies sont
/// perimes. Ce script ne fait que declencher le Refresh ; le rechargement de domaine qui suit peut
/// interrompre la reponse, c'est attendu.
/// </summary>
public static class Story516ForceAssetRefresh
{
    public static string Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        return "AssetDatabase.Refresh(ForceUpdate) demande.";
    }
}
