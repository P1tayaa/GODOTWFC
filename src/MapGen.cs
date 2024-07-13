using Godot;
using GodotPlugins.Game;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using System.Text.Json;

public partial class MapGen : Node2D
{

	int	num_type_tiles = 17;
	public	Terain_Type[]	current_types;
	public	class	Neighbers {
		public	string[]	left { get; set; }
		public	string[]	right { get; set; }
		public	string[]	down { get; set; }
		public	string[]	up { get; set; }
	}

	public	class	Neighbers_Int {
		public	int[]	left { get; set; }
		public	int[]	right { get; set; }
		public	int[]	down { get; set; }
		public	int[]	up { get; set; }
	}

	public	class	Terain_Type {
		public	string	Name { get; set; }
		public	Point	Point { get; set; }
		public	int		Weight {get; set; }
		public	Neighbers	Neighber {get; set; }
		public	Neighbers_Int	Neighber_int {get; set; }
	}

	public Terain_Type[]	get_terain_type() {
		Terain_Type[] temp;
		Terain_Type[] final;
		Godot.FileAccess files;
		files = Godot.FileAccess.Open("res://src/Terain_type.json", Godot.FileAccess.ModeFlags.Read);
		string text = files.GetAsText();
		temp = JsonSerializer.Deserialize<Terain_Type[]>(text);
		final = (Terain_Type[])temp.Clone();
		GD.Print(final[0].Point);
		if (num_type_tiles != final.Length)
			GD.Print("!tiles num is wrong!");
		return (final);
	}

	public	int get_total_num_type(Terain_Type[] terain_type) {
		return(terain_type.Length);
	}


	class   Cell_Type {
		public bool[]  types;
		public int     final_type;
		public	int	entrepie;
			
		public void clone_cell(Cell_Type cell) 	{
			types = cell.types;
			entrepie = cell.entrepie;
		}

		public	bool	visited = false;

		public Cell_Type(int num_type)	{
			types = new	bool[num_type];
			for (int i = 0; i < num_type; i++) {
				types[i] = true;
			}
			entrepie = num_type;
			final_type = -1;
		}

		public	void	UpdateEntrepie() {
			int new_entrepie = 0;
			foreach (var item in types) {
				if (item)
					new_entrepie++;
			}
			entrepie = new_entrepie;
		}
		public void	Collapse() {
			var trueIndices_1 = types.Select((value, index) => new { value, index })
								   .Where(pair => pair.value)
								   .Select(pair => pair.index);
			int random_index_select = (int)Math.Floor(GD.RandRange(0, (double)(entrepie)));
			final_type = trueIndices_1.ElementAt(random_index_select);
			entrepie = 0;
			for (int j = 0; j < types.Length; j++) {
				types[j] = false;
			}
			types[final_type] = true;
			return ;
		}
	}

	class Wave_Function_Collapse {

		public	Cell_Type[][] Cells;

		private	Cell_Type	get_cell_at(Point past_coord) {
			return Cells[past_coord.Y][past_coord.X];
		}

		public	int	x_size;
		public	int	y_size;
		public Terain_Type[]	current_types;

		public Wave_Function_Collapse(int new_x_size, int new_y_size, int num_type_tiles) {
			x_size = new_x_size;
			y_size = new_y_size;
			Cells = new Cell_Type[y_size][];
			for (int i = 0; i < y_size; i++) {
				Cells[i] = new Cell_Type[x_size];
				for (int j = 0; j < x_size; j++) {
					Cells[i][j] = new Cell_Type(num_type_tiles);
				}
			}
		}

		private	bool	isAllCollapse() {
			foreach (var y_item in Cells) {
				foreach (var x_item in y_item) {
					if (x_item.final_type == -1)
						return true;
				}
			}
			return false;
		}

		private bool[]	Reduce_entrpie_just_me_down(Point curent_coord) {
			int	loop_lenght = get_cell_at(curent_coord).types.Length;
			bool[] local_possible = new	bool[loop_lenght];
			for (int i = 0; i < loop_lenght; i++) {
				local_possible[i] = false;
			}
			var trueIndices_1 = Cells[curent_coord.Y + 1][curent_coord.X].types.Select((value, index) => new { value, index })
								   .Where(pair => pair.value)
								   .Select(pair => pair.index);
			foreach (int index in trueIndices_1) {
				foreach (var neighber in current_types[index].Neighber.up) {
					local_possible[get_index_of_type_cell_of_name(neighber)] = true;
				}
			}
			return local_possible;
		}

		private bool[]	Reduce_entrpie_just_me_up(Point curent_coord) {
			int	loop_lenght = get_cell_at(curent_coord).types.Length;
			bool[] local_possible = new	bool[loop_lenght];
			for (int i = 0; i < loop_lenght; i++) {
				local_possible[i] = false;
			}
			var trueIndices_1 = Cells[curent_coord.Y - 1][curent_coord.X].types.Select((value, index) => new { value, index })
								   .Where(pair => pair.value)
								   .Select(pair => pair.index);
			foreach (int index in trueIndices_1) {
				foreach (var neighber in current_types[index].Neighber.down) {
						local_possible[get_index_of_type_cell_of_name(neighber)] = true;
				}
			}
			return local_possible;
		}

		private bool[]	Reduce_entrpie_just_me_left(Point curent_coord) {
			int	loop_lenght = get_cell_at(curent_coord).types.Length;
			bool[] local_possible = new	bool[loop_lenght];
			for (int i = 0; i < loop_lenght; i++) {
				local_possible[i] = false;
			}
			var trueIndices_1 = Cells[curent_coord.Y][curent_coord.X - 1].types.Select((value, index) => new { value, index })
								   .Where(pair => pair.value)
								   .Select(pair => pair.index);
			foreach (int index in trueIndices_1) {
				foreach (var neighber in current_types[index].Neighber.right) {
						local_possible[get_index_of_type_cell_of_name(neighber)] = true;
				}
			}
			return local_possible;
		}

		private bool[]	Reduce_entrpie_just_me_right(Point curent_coord) {
			int	loop_lenght = get_cell_at(curent_coord).types.Length;
			bool[] local_possible = new	bool[loop_lenght];
			for (int i = 0; i < loop_lenght; i++) {
				local_possible[i] = false;
			}
			var trueIndices_1 = Cells[curent_coord.Y][curent_coord.X + 1].types.Select((value, index) => new { value, index })
								   .Where(pair => pair.value)
								   .Select(pair => pair.index);
			foreach (int index in trueIndices_1) {
				foreach (var neighber in current_types[index].Neighber.left) {
						local_possible[get_index_of_type_cell_of_name(neighber)] = true;
				}
			}
			return local_possible;
		}


		private	bool[]	all_true_bool_return(Point curent_coord) {
			bool[] local_possible = new	bool[get_cell_at(curent_coord).types.Length];
			for (int i = 0; i < get_cell_at(curent_coord).types.Length; i++)
			{
				local_possible[i] = true;
			}
			return local_possible;
		}

		private	bool	Reduce_entrpie_just_me(Point curent_coord) {
			bool[]	down_possible;
			bool[]	up_possible;
			bool[]	left_possible;
			bool[]	right_possible;
			bool	change = false;
			if (curent_coord.Y < y_size - 1) {
				down_possible = Reduce_entrpie_just_me_down(curent_coord);
			}
			else
				down_possible = all_true_bool_return(curent_coord);
			if (curent_coord.X < x_size - 1) {
				right_possible = Reduce_entrpie_just_me_right(curent_coord);
			}
			else
				right_possible = all_true_bool_return(curent_coord);
			if (curent_coord.Y > 0) {
				up_possible = Reduce_entrpie_just_me_up(curent_coord);
			}
			else
				up_possible = all_true_bool_return(curent_coord);
			if (curent_coord.X > 0) {
				left_possible = Reduce_entrpie_just_me_left(curent_coord);
			}
			else
				left_possible = all_true_bool_return(curent_coord);
			for (int i = 0; i < down_possible.Length; i++) {
				if (Cells[curent_coord.Y][curent_coord.X].types[i] && down_possible[i] && up_possible[i] && right_possible[i] && left_possible[i])
				{
					if (!Cells[curent_coord.Y][curent_coord.X].types[i]) {
						Cells[curent_coord.Y][curent_coord.X].types[i] = true;
						change = true;
					}
				}
				else
					if (Cells[curent_coord.Y][curent_coord.X].types[i]) {
						Cells[curent_coord.Y][curent_coord.X].types[i] = false;
						change = true;
					}
			}
			return (change);
		}

		public	void	Collapse_cell(Point curent_coord) {
			if (Cells[curent_coord.Y][curent_coord.X].entrepie == 0)
				GD.Print($"bad before {curent_coord}");
			// Reduce_entrpie_just_me(curent_coord);
			int new_entrepie = 0;
			foreach (var item in Cells[curent_coord.Y][curent_coord.X].types) {
				if (item)
					new_entrepie++;
			}
			Cells[curent_coord.Y][curent_coord.X].entrepie = new_entrepie;
			Cells[curent_coord.Y][curent_coord.X].Collapse();
			if (Cells[curent_coord.Y][curent_coord.X].final_type == 16)
				GD.Print($"bad after {curent_coord}");
		}

		private	Point	find_low_entrepie() {
			Point	index;
			int			lowest_entrepie = int.MaxValue;
		
			index = new Point(-1, -1);
			for (int i = 0; i < y_size; i++) {
				for (int j = 0; j < x_size; j++) {
					if (Cells[i][j].entrepie < lowest_entrepie && Cells[i][j].final_type == -1) {
						lowest_entrepie = Cells[i][j].entrepie;
						index.X = j;
						index.Y = i;
					}
				}
			}
			return index;
		}

		public	int	get_index_of_type_cell_of_name(string	name) {
			int i = 0;
			foreach (var item in current_types) {
				if (string.Compare(item.Name, name) == 0)
					return (i);
				i++;
			}
			return (16);
		}


		// need to make it recursive for late when there is more rules
		// private void	Propagate_entrepie_to_other_cells(Point	curent_coord, bool first_run, Point	past_coord)
		private void	Propagate_entrepie_to_other_cells(Point	curent_coord, bool	first) {
			Stack	need_to_probagate = new	Stack();
			need_to_probagate.Push(curent_coord);
			while (need_to_probagate.Count > 0) {
				curent_coord = (Point)need_to_probagate.Pop();
				int old_entrepie = get_cell_at(curent_coord).entrepie;
				if (get_cell_at(curent_coord).entrepie > 1) {
					Reduce_entrpie_just_me(curent_coord);
				}
				get_cell_at(curent_coord).UpdateEntrepie();
				if (old_entrepie != get_cell_at(curent_coord).entrepie) {
					if (curent_coord.X < x_size - 1) {
						Point	new_point = new Point(curent_coord.X + 1, curent_coord.Y);
						if (!need_to_probagate.Contains(new_point))
							need_to_probagate.Push(new_point);
					}
					if (curent_coord.Y < y_size - 1) {
						Point	new_point = new Point(curent_coord.X, curent_coord.Y + 1);
						if (!need_to_probagate.Contains(new_point))
							need_to_probagate.Push(new_point);
					}
					if (curent_coord.X > 0) {
						Point	new_point = new Point(curent_coord.X - 1, curent_coord.Y);
						if (!need_to_probagate.Contains(new_point))
							need_to_probagate.Push(new_point);
					}
					if (curent_coord.Y > 0)	{
						Point	new_point = new Point(curent_coord.X, curent_coord.Y - 1);
						if (!need_to_probagate.Contains(new_point))
							need_to_probagate.Push(new_point);
					}
				}
			}
		}

		private void 	unvite_cells() {
			for (int i = 0; i < y_size; i++) {
				for (int j = 0; j < x_size; j++) {
					Cells[i][j].visited = false;
				}
			}
		}

		private	void	print_map_new_entrepie() {
			string	text;
			for (int y = 0; y < y_size; y++) {
				text = "";
				for (int x = 0; x < x_size; x++) {
					text = text + ($"{Cells[y][x].entrepie}, ");
				}
				GD.Print(text);
			}
		}

		private	void	check_types_side_right_name_json() {
			for (int i = 0; i < current_types.Length; i++)
			{
				int	total_type = 0;
				foreach (var item in current_types[i].Neighber.up)
				{
					bool	isthere = false;
					foreach (var item2 in current_types[get_index_of_type_cell_of_name(item)].Neighber.down)
					{
						if (string.Compare(current_types[i].Name, item2) == 0)
							isthere = true;
					}
					if (!isthere)
						throw new DivideByZeroException($"Cell infor not corect up for {current_types[get_index_of_type_cell_of_name(item)].Name} and {current_types[i].Name}");
					total_type++;
				}
				GD.Print($"{current_types[i].Name} up has total of {total_type} type");
				total_type = 0;
				foreach (var item in current_types[i].Neighber.down)
				{
					bool	isthere = false;
					foreach (var item2 in current_types[get_index_of_type_cell_of_name(item)].Neighber.up)
					{
						if (string.Compare(current_types[i].Name, item2) == 0)
							isthere = true;
					}
					if (!isthere)
						throw new DivideByZeroException($"Cell infor not corect down for {current_types[get_index_of_type_cell_of_name(item)].Name} and {current_types[i].Name}");
					total_type++;
				}
				GD.Print($"{current_types[i].Name} down has total of {total_type} type");
				total_type = 0;
				foreach (var item in current_types[i].Neighber.right)
				{
					bool	isthere = false;
					foreach (var item2 in current_types[get_index_of_type_cell_of_name(item)].Neighber.left)
					{
						if (string.Compare(current_types[i].Name, item2) == 0)
							isthere = true;
					}
					if (!isthere)
						throw new DivideByZeroException($"Cell infor not corect right for {current_types[get_index_of_type_cell_of_name(item)].Name} and {current_types[i].Name}");
					total_type++;
				}
				GD.Print($"{current_types[i].Name} right has total of {total_type} type");
				total_type = 0;
				foreach (var item in current_types[i].Neighber.left)
				{
					bool	isthere = false;
					foreach (var item2 in current_types[get_index_of_type_cell_of_name(item)].Neighber.right)
					{
						if (string.Compare(current_types[i].Name, item2) == 0)
							isthere = true;
					}
					if (!isthere)
						throw new DivideByZeroException($"Cell infor not corect left for {current_types[get_index_of_type_cell_of_name(item)].Name} and {current_types[i].Name}");
					total_type++;
				}
				GD.Print($"{current_types[i].Name} left has total of {total_type} type\n");
			}
		}

		public	void	WFC_main() {
			Point	curent_coord;

			while (isAllCollapse()) {
				curent_coord = find_low_entrepie();
				Collapse_cell(curent_coord);
				Propagate_entrepie_to_other_cells(curent_coord, true);
				int new_entrepie = 0;
				foreach (var item in Cells[curent_coord.Y][curent_coord.X].types) {
					if (item)
						new_entrepie++;
				}
				Cells[curent_coord.Y][curent_coord.X].entrepie = new_entrepie;
				unvite_cells();
			}
		}

		public	Vector2I Get_right_cell_type(int x, int y) {
			int	cell_type = Cells[y][x].final_type;
			Terain_Type	current_point;

			if (Cells[y][x].final_type == -1)
				current_point = current_types[16];
			else
				current_point = current_types[cell_type];
			int new_x = current_point.Point.X;
			int new_y = current_point.Point.Y;
			return (new Vector2I(new_x, new_y));
		}
	}

	[Export]
	public  bool start = false;
	[Export]
	public  Vector2I WFC_size;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (start == true)
		{
			start = false;
			Wave_Function_Collapse	Model = new	Wave_Function_Collapse(WFC_size.X, WFC_size.Y, num_type_tiles);
			Model.current_types = get_terain_type();
			
			Model.WFC_main();
			TileMap tile_map = GetChild<TileMap>(0);
			
			for (int i = 0; i < Model.y_size; i++)
			{
				for (int j = 0; j < Model.x_size; j++)
				{
					tile_map.SetCell(0, new Vector2I(j, i), 0, Model.Get_right_cell_type(j, i));
				}
			}
		}
	}
}
